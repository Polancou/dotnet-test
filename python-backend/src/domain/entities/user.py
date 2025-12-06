from sqlalchemy import Column, String, Enum as SqlEnum, DateTime
from src.domain.common.base_entity import BaseEntity
from src.domain.enums.user_role import UserRole
from datetime import datetime

class User(BaseEntity):
    """
    User entity/model representing a registered user in the application.

    Inherits from BaseEntity, providing:
    - Standard id, is_active, and audit timestamps.

    Fields:
        username (str): Unique username for user login and identification.
        email (str): Email address of the user.
        password_hash (str): Hashed password string (never store plain text).
        role (UserRole): Role of the user (e.g., ADMIN, USER).
        refresh_token (str, optional): Optional current refresh token for authentication.
        refresh_token_expiry_time (datetime, optional): Expiry time for refresh token.

    Methods:
        update_password(new_password_hash): Update the user's password hash.
        update_refresh_token(refresh_token, expiry_time): Update refresh token and its expiry.
        update_role(new_role): Change the user's role.
    """

    __tablename__ = "users"

    # Username for the user; required and unique (enforce uniqueness at the DB layer if required)
    username = Column(String, nullable=False)

    # User's email address; required (consider adding unique=True if emails are required to be unique)
    email = Column(String, nullable=False)

    # Hashed password for the user
    password_hash = Column(String, nullable=False)

    # User role, stored as an enum in the DB
    role = Column(SqlEnum(UserRole), nullable=False)

    # Current refresh token for session management (optional)
    refresh_token = Column(String, nullable=True)

    # Expiry time of the refresh token (optional)
    refresh_token_expiry_time = Column(DateTime, nullable=True)

    def __init__(self, username: str, email: str, password_hash: str, role: UserRole):
        """
        Initialize a new User entity with required fields.

        Args:
            username (str): Unique username for the user. Cannot be empty.
            email (str): Email address of the user. Cannot be empty.
            password_hash (str): Hashed password. Cannot be empty.
            role (UserRole): Role assigned to the user.

        Raises:
            ValueError: If any required argument is empty.
        """
        # Validate that username is provided
        if not username:
            raise ValueError("Username cannot be empty")
        # Validate that email is provided
        if not email:
            raise ValueError("Email cannot be empty")
        # Validate that password hash is provided
        if not password_hash:
            raise ValueError("Password hash cannot be empty")
        
        # Assign basic user attributes
        self.username = username
        self.email = email
        self.password_hash = password_hash
        self.role = role
        # Initialize the BaseEntity, marking user as active by default
        super().__init__(is_active=True)

    def update_password(self, new_password_hash: str):
        """
        Updates the user's password hash. Also updates the modification timestamp.

        Args:
            new_password_hash (str): The new password hash. Cannot be empty.

        Raises:
            ValueError: If the new password hash is empty.
        """
        if not new_password_hash:
            raise ValueError("New password hash cannot be empty")
        # Update the password hash and modification time
        self.password_hash = new_password_hash
        self.update_modification_date()

    def update_refresh_token(self, refresh_token: str, expiry_time: datetime):
        """
        Updates the user's refresh token and its expiry time.

        Args:
            refresh_token (str): The new refresh token.
            expiry_time (datetime): The refresh token's expiry time.
        """
        # Set new refresh token and its expiry, then update audit info
        self.refresh_token = refresh_token
        self.refresh_token_expiry_time = expiry_time
        self.update_modification_date()

    def update_role(self, new_role: UserRole):
        """
        Updates the user's role. Also updates the modification timestamp.

        Args:
            new_role (UserRole): The new role to assign to the user.
        """
        # Change the user's role and update audit info
        self.role = new_role
        self.update_modification_date()
