"""
This module provides service implementations for security-related operations such as
password hashing/validation and JWT generation. These services are typically injected
for authentication purposes throughout the application.
"""

import bcrypt
from datetime import datetime, timedelta
import jwt
from src.application.interfaces.interfaces import IPasswordHasher, IJwtService
from src.domain.entities.user import User
from src.config import settings

# Load JWT configuration from settings
SECRET_KEY = settings.JWT_SECRET_KEY                   # Secret key for JWT signing
ALGORITHM = settings.JWT_ALGORITHM                     # Algorithm (e.g., "HS256")
ACCESS_TOKEN_EXPIRE_MINUTES = settings.ACCESS_TOKEN_EXPIRE_MINUTES  # Access token expiry

class PasswordHasher(IPasswordHasher):
    """
    Secure password hashing and verification using bcrypt.
    Implements the IPasswordHasher application interface.
    """
    def hash(self, password: str) -> str:
        """
        Hash a plaintext password using bcrypt.

        Args:
            password (str): The user-provided plaintext password.

        Returns:
            str: A bcrypt hash of the password.
        """
        # bcrypt accepts input as bytes, so encode the password
        pwd_bytes = password.encode('utf-8')
        # Generate a cryptographically secure salt
        salt = bcrypt.gensalt()
        # Hash the password using the salt
        hashed = bcrypt.hashpw(pwd_bytes, salt)
        # Return the hash as a UTF-8 string for storage/validation
        return hashed.decode('utf-8')

    def verify(self, password: str, password_hash: str) -> bool:
        """
        Verify a plaintext password against a bcrypt hash.

        Args:
            password (str): Plaintext password to check.
            password_hash (str): The bcrypt hash to check against.

        Returns:
            bool: True if password matches hash, False otherwise.
        """
        # Both inputs must be bytes for bcrypt.checkpw
        pwd_bytes = password.encode('utf-8')
        hash_bytes = password_hash.encode('utf-8')
        # Returns True if the password matches the hash
        return bcrypt.checkpw(pwd_bytes, hash_bytes)

class JwtService(IJwtService):
    """
    Service for generating JWTs for users (access and refresh tokens).
    Implements the IJwtService interface.
    """
    def generate_access_token(self, user: User) -> str:
        """
        Generate a signed JWT (access token) representing the user's identity and role.

        Args:
            user (User): The user entity for whom to generate the token.

        Returns:
            str: A JWT access token as an encoded string.
        """
        # Build the claims payload for the access token
        to_encode = {
            "sub": user.username,                        # Subject (user identity)
            "id": user.id,                               # User ID
            "role": user.role.value,                     # User role as string
            # Set token expiration
            "exp": datetime.utcnow() + timedelta(minutes=ACCESS_TOKEN_EXPIRE_MINUTES)
        }
        # Encode and sign the JWT using the application's secret key & algorithm
        encoded_jwt = jwt.encode(to_encode, SECRET_KEY, algorithm=ALGORITHM)
        return encoded_jwt

    def generate_refresh_token(self) -> str:
        """
        Generate a secure random refresh token string.

        Returns:
            str: A cryptographically secure random string (URL-safe).
        """
        # Use Python's secrets for strong randomness (suitable for refresh tokens)
        import secrets
        return secrets.token_urlsafe(32)
