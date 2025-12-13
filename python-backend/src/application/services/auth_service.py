from datetime import datetime, timedelta
from typing import Optional
from src.application.interfaces.interfaces import (
    IAuthService,
    IUserRepository,
    IJwtService,
    IUnitOfWork,
    IPasswordHasher,
    IEventLogService,
)
from src.application.dtos.auth_dtos import LoginRequest, LoginResponse, RegisterRequest
from src.domain.entities.user import User
from src.domain.enums.user_role import UserRole

class AuthService(IAuthService):
    """
    Authentication service implementing user login, registration, and token refresh.

    This service coordinates user authentication, password verification,
    JWT handling, refresh token lifecycle, and logs authentication events.
    """

    def __init__(
        self,
        user_repository: IUserRepository,
        jwt_service: IJwtService,
        unit_of_work: IUnitOfWork,
        password_hasher: IPasswordHasher,
        event_log_service: IEventLogService,
    ):
        """
        Initialize the AuthService with required dependencies.

        Args:
            user_repository (IUserRepository): Repository for user data access.
            jwt_service (IJwtService): Service for JWT token handling.
            unit_of_work (IUnitOfWork): Ensures transactional integrity.
            password_hasher (IPasswordHasher): For password hashing/verification.
            event_log_service (IEventLogService): For logging user events.
        """
        self.user_repository = user_repository
        self.jwt_service = jwt_service
        self.unit_of_work = unit_of_work
        self.password_hasher = password_hasher
        self.event_log_service = event_log_service

    async def login(self, request: LoginRequest) -> LoginResponse:
        """
        Authenticates the user and issues new JWT & refresh tokens.

        Verifies username/password, generates tokens, updates the user's refresh token,
        commits the changes, and logs the login event.

        Args:
            request (LoginRequest): Encapsulates username and password.

        Returns:
            LoginResponse: Contains new access and refresh tokens, expiry, and user role.

        Raises:
            Exception: If credentials are invalid.
        """
        # Retrieve user by username
        user = self.user_repository.get_by_username(request.username)
        # Check user existence and verify password hash
        if not user or not self.password_hasher.verify(request.password, user.password_hash):
            raise Exception("Invalid credentials")  # For production, replace with custom exception

        # Generate tokens
        access_token = self.jwt_service.generate_access_token(user)
        refresh_token = self.jwt_service.generate_refresh_token()

        # Update user's refresh token and expiry (valid for 7 days)
        user.update_refresh_token(refresh_token, datetime.utcnow() + timedelta(days=7))
        self.unit_of_work.commit()

        # Log successful login
        await self.event_log_service.log_event(
            "User Interaction", f"User {user.username} logged in.", user.id
        )

        # Return authentication response
        return LoginResponse(
            access_token=access_token,
            refresh_token=refresh_token,
            expires_in_minutes=15,
            role=user.role.value
        )

    async def register(self, request: RegisterRequest):
        """
        Registers a new user in the system.

        Checks for duplicate usernames, hashes the password, creates and persists user entity.

        Args:
            request (RegisterRequest): Includes username, email, and password.

        Raises:
            Exception: If the username already exists.
        """
        # Check for username uniqueness
        existing_user = self.user_repository.get_by_username(request.username)
        if existing_user:
            raise Exception("Username already exists")

        # Hash the user's password
        password_hash = self.password_hasher.hash(request.password)
        # Instantiate user entity with default USER role
        user = User(request.username, request.email, password_hash, UserRole.USER)
        
        # Add the new user and commit transaction
        self.user_repository.add(user)
        self.unit_of_work.commit()

    async def refresh_token(self, token: str) -> LoginResponse:
        """
        Refreshes access and refresh tokens using a valid refresh token.

        Validates the refresh token, ensures it hasn't expired, and generates new tokens.
        Updates refresh token info in the database.

        Args:
            token (str): The refresh token provided by the client.

        Returns:
            LoginResponse: Contains new access and refresh tokens, expiry, and user role.

        Raises:
            Exception: If the refresh token is invalid or expired.
        """
        # Retrieve user by refresh token
        user = self.user_repository.get_by_refresh_token(token)
        # Check token validity and expiry
        if not user or (
            user.refresh_token_expiry_time
            and user.refresh_token_expiry_time <= datetime.utcnow()
        ):
            raise Exception("Invalid or expired refresh token")

        # Generate new tokens
        new_access_token = self.jwt_service.generate_access_token(user)
        new_refresh_token = self.jwt_service.generate_refresh_token()

        # Update refresh token and expiry (7 days from now)
        user.update_refresh_token(new_refresh_token, datetime.utcnow() + timedelta(days=7))
        self.unit_of_work.commit()

        # Return new tokens
        return LoginResponse(
            access_token=new_access_token,
            refresh_token=new_refresh_token,
            expires_in_minutes=15,
            role=user.role.value
        )
