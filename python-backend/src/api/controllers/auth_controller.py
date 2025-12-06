"""
auth_controller.py

Defines the API endpoints for user authentication, including login, registration,
and token refreshing. Utilizes dependency-injected authentication service.
Provides detailed exception handling and HTTP status code mapping.
"""

from fastapi import APIRouter, Depends, HTTPException
from src.application.interfaces.interfaces import IAuthService
from src.application.dtos.auth_dtos import LoginRequest, LoginResponse, RegisterRequest, RefreshTokenRequest
from src.api.dependencies import get_auth_service

# Create an API router for authentication-related routes
router = APIRouter(prefix="/auth", tags=["Auth"])

@router.post("/login", response_model=LoginResponse)
def login(request: LoginRequest, auth_service: IAuthService = Depends(get_auth_service)):
    """
    Authenticate a user and return a JWT access/refresh token pair.

    Args:
        request (LoginRequest): The login credentials from the client.
        auth_service (IAuthService): Auth service for performing authentication.

    Returns:
        LoginResponse: JWT tokens and user info on success.

    Raises:
        HTTPException: 401 if credentials are invalid.
    """
    try:
        # Attempt to authenticate the user using provided credentials
        return auth_service.login(request)
    except Exception as e:
        # Authentication failed (invalid user, wrong password, etc.)
        raise HTTPException(status_code=401, detail=str(e))

@router.post("/register")
def register(request: RegisterRequest, auth_service: IAuthService = Depends(get_auth_service)):
    """
    Register a new user in the system.

    Args:
        request (RegisterRequest): Information required to create a new user.
        auth_service (IAuthService): Service for registering users.

    Returns:
        dict: Success message on successful registration.

    Raises:
        HTTPException: 400 on registration failure (e.g., user already exists).
    """
    try:
        # Attempt to register the new user
        auth_service.register(request)
        return {"message": "User registered successfully"}
    except Exception as e:
        # Registration failed (user exists, validation error, etc.)
        raise HTTPException(status_code=400, detail=str(e))

@router.post("/refresh", response_model=LoginResponse)
def refresh_token(request: RefreshTokenRequest, auth_service: IAuthService = Depends(get_auth_service)):
    """
    Refresh an access token using a valid refresh token.

    Args:
        request (RefreshTokenRequest): Contains the refresh token.
        auth_service (IAuthService): Service for refreshing tokens.

    Returns:
        LoginResponse: New JWT access/refresh token pair.

    Raises:
        HTTPException: 401 on invalid or expired refresh token.
    """
    try:
        # Attempt to refresh the JWT tokens using the provided refresh token
        return auth_service.refresh_token(request.refresh_token)
    except Exception as e:
        # Token refresh failed (invalid/expired token)
        raise HTTPException(status_code=401, detail=str(e))
