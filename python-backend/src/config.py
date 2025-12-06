"""
Configuration module for the application.

This module loads environment variables (from a .env file if present) and exposes them
in a strongly-typed, class-based settings object. All environment-driven configuration
(e.g., database connection details, secret keys, timeouts, and third-party API keys)
are managed from here.

Usage:
    from src.config import settings
    db_url = settings.DATABASE_URL
"""

import os
from dotenv import load_dotenv

# Load environment variables from a .env file into the process environment.
# This allows local overrides for development or deployment.
load_dotenv()

class Settings:
    """
    Application settings loaded from environment variables or defaults.

    Attributes:
        DATABASE_URL (str): The database connection string. Defaults to SQLite local file if not provided.
        JWT_SECRET_KEY (str): Secret key for signing JWT tokens. Critical for security!
        JWT_ALGORITHM (str): The JWT signing algorithm, e.g., 'HS256'.
        ACCESS_TOKEN_EXPIRE_MINUTES (int): How long access JWTs are valid (in minutes).
        GEMINI_API_KEY (str | None): Gemini AI API key for accessing Google's generative AI.
    """
    # --- Database Connection ---
    DATABASE_URL = os.getenv("DATABASE_URL", "sqlite:///./app.db")  # Use SQLite DB as fallback
    
    # --- JWT & Authentication ---
    JWT_SECRET_KEY = os.getenv("JWT_SECRET_KEY", "supersecretkey")  # Should be overridden in production
    JWT_ALGORITHM = os.getenv("JWT_ALGORITHM", "HS256")  # Symmetric HMAC SHA-256
    ACCESS_TOKEN_EXPIRE_MINUTES = int(os.getenv("ACCESS_TOKEN_EXPIRE_MINUTES", 15))  # Token expiry in minutes
    
    # --- 3rd Party Services (e.g., Google Gemini AI) ---
    GEMINI_API_KEY = os.getenv("GEMINI_API_KEY")  # May be None if not configured

# Instantiate a global settings instance for import throughout the codebase.
settings = Settings()
