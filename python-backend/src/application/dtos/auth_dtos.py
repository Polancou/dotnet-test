from pydantic import BaseModel, Field

# -----------------------------------------------------------------------------
# Data Transfer Objects (DTOs) for Authentication
#
# These classes define the structure for payloads sent to and received from the
# authentication endpoints (login, registration, and token refresh).
# They ensure consistent, well-documented API contracts for auth workflows.
# -----------------------------------------------------------------------------

class LoginRequest(BaseModel):
    """
    Request schema for user login.

    Attributes:
        username (str): The user's username for identification.
        password (str): The plaintext password submitted for authentication.
    """
    username: str  # Username credential for login
    password: str  # Plaintext password credential

class LoginResponse(BaseModel):
    """
    Response schema sent after a successful login.

    Attributes:
        access_token (str): JWT for short-term API access (returned as 'accessToken').
        refresh_token (str): Token allowing access token renewal (returned as 'refreshToken').
        expires_in_minutes (int): Minutes until the access token expires (alias 'expiresInMinutes').
        role (str): User's role (e.g., "admin", "user").
    """
    access_token: str = Field(serialization_alias="accessToken")  # JWT access token for requests
    refresh_token: str = Field(serialization_alias="refreshToken")  # Token to refresh access token
    expires_in_minutes: int = Field(serialization_alias="expiresInMinutes")  # Expiry duration
    role: str  # Role/privilege assigned to user

class RegisterRequest(BaseModel):
    """
    Request schema for registering a new user.

    Attributes:
        username (str): Chosen username.
        email (str): Valid user email address.
        password (str): Secure password.
    """
    username: str  # Desired username
    email: str     # User's email address
    password: str  # Desired password

class RefreshTokenRequest(BaseModel):
    """
    Request schema for refreshing an expired access token.

    Attributes:
        refresh_token (str): Refresh token used to obtain a new access token.
    """
    refresh_token: str = Field(alias="refreshToken")  # Provided refresh token for renewal
