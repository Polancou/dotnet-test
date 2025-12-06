"""
dependencies.py

Dependency provider functions for FastAPI's DI system.

This module contains factory functions for instantiating repositories, services,
and utility classes as dependencies in FastAPI routes and other services.

It also centralizes authentication logic such as retrieving the current user from JWT.
"""

from fastapi import Depends, HTTPException, status
from fastapi.security import OAuth2PasswordBearer
import jwt
from jwt.exceptions import PyJWTError
from sqlalchemy.orm import Session

from src.infrastructure.persistence.database import get_db
from src.infrastructure.repositories.repositories import UserRepository, EventLogRepository, DocumentRepository
from src.infrastructure.persistence.unit_of_work import UnitOfWork
from src.infrastructure.services.services_impl import PasswordHasher, JwtService
from src.config import settings
from src.infrastructure.services.file_storage_service import FileStorageService
from src.infrastructure.services.s3_file_storage_service import S3FileStorageService
from src.config import settings
from src.infrastructure.services.ai_analysis_service import AiAnalysisService
from src.application.services.auth_service import AuthService
from src.application.services.user_service import UserService
from src.application.services.event_log_service import EventLogService
from src.application.services.document_service import DocumentService
from src.application.services.csv_service import CsvService

# OAuth2 scheme used by FastAPI's security system.
# This will look for an "Authorization: Bearer ..." header.
oauth2_scheme = OAuth2PasswordBearer(tokenUrl="auth/login")

# --- Repository dependencies ---

def get_user_repository(db: Session = Depends(get_db)):
    """
    Dependency provider for UserRepository. Resolves an active DB session from get_db().
    """
    return UserRepository(db)

def get_event_log_repository(db: Session = Depends(get_db)):
    """
    Dependency provider for EventLogRepository using an active DB session.
    """
    return EventLogRepository(db)

def get_document_repository(db: Session = Depends(get_db)):
    """
    Dependency provider for DocumentRepository using an active DB session.
    """
    return DocumentRepository(db)

# --- Unit of Work dependency ---

def get_unit_of_work(db: Session = Depends(get_db)):
    """
    Provides a UnitOfWork for transactional control in services.
    """
    return UnitOfWork(db)

# --- Utility/service (stateless or singleton) dependencies ---

def get_password_hasher():
    """
    Dependency provider for PasswordHasher utility (for password hashing).
    """
    return PasswordHasher()

def get_jwt_service():
    """
    Dependency provider for JwtService utility (for JWT encode/decode).
    """
    return JwtService()

def get_file_storage_service():
    """
    Dependency provider for FileStorageService (handles file uploads and persistence).
    Uses S3 storage if configured, otherwise falls back to local filesystem storage.
    """
    if settings.USE_S3_STORAGE:
        try:
            return S3FileStorageService()
        except (ValueError, Exception) as e:
            # If S3 configuration is invalid, fall back to local storage
            import logging
            logger = logging.getLogger(__name__)
            logger.warning(f"Failed to initialize S3 storage, falling back to local storage: {str(e)}")
            return FileStorageService()
    else:
        return FileStorageService()

# --- Authentication dependency ---

def get_current_user(
    token: str = Depends(oauth2_scheme),
    user_repo=Depends(get_user_repository)
):
    """
    Dependency to resolve the current user based on a JWT access token.

    - Decodes the token.
    - Extracts the "sub" (subject) field, which should be the username.
    - Looks up the user in the database.
    - Raises 401 if authentication fails.

    Returns:
        User instance
    """
    credentials_exception = HTTPException(
        status_code=status.HTTP_401_UNAUTHORIZED,
        detail="Could not validate credentials",
        headers={"WWW-Authenticate": "Bearer"},
    )
    try:
        # Decode JWT token and extract payload
        payload = jwt.decode(token, settings.JWT_SECRET_KEY, algorithms=[settings.JWT_ALGORITHM])
        username: str = payload.get("sub")
        if username is None:
            # If there's no username in the payload, reject authentication
            raise credentials_exception
    except PyJWTError:
        # Handle JWT decode or signature errors
        raise credentials_exception
    
    # Fetch user from repository
    user = user_repo.get_by_username(username)
    if user is None:
        # If user not found in the DB, reject authentication
        raise credentials_exception
    return user

# --- Service dependencies (aggregating repositories, utilities, etc.) ---

def get_event_log_service(
    event_log_repo=Depends(get_event_log_repository),
    uow=Depends(get_unit_of_work)
):
    """
    Constructs and provides an EventLogService with all required dependencies.
    """
    return EventLogService(event_log_repo, uow)

def get_ai_analysis_service(
    event_log_service=Depends(get_event_log_service)
):
    """
    Constructs an AI analysis service, which handles AI-powered document analysis.
    """
    return AiAnalysisService(event_log_service)

def get_csv_service(
    user_repo=Depends(get_user_repository),
    uow=Depends(get_unit_of_work),
    password_hasher=Depends(get_password_hasher)
):
    """
    Provides a CsvService for user and password hashing batch operations.
    """
    return CsvService(user_repo, uow, password_hasher)

def get_document_service(
    document_repo=Depends(get_document_repository),
    uow=Depends(get_unit_of_work),
    file_storage_service=Depends(get_file_storage_service),
    csv_service=Depends(get_csv_service),
    event_log_service=Depends(get_event_log_service)
):
    """
    Constructs a DocumentService with dependencies for document processing,
    storage, CSV management, and event logging.
    """
    return DocumentService(document_repo, uow, file_storage_service, csv_service, event_log_service)

def get_auth_service(
    user_repo=Depends(get_user_repository),
    jwt_service=Depends(get_jwt_service),
    uow=Depends(get_unit_of_work),
    password_hasher=Depends(get_password_hasher),
    event_log_service=Depends(get_event_log_service)
):
    """
    Provides an AuthService for authentication, registration, login, and event logging.
    """
    return AuthService(user_repo, jwt_service, uow, password_hasher, event_log_service)

def get_user_service(
    user_repo=Depends(get_user_repository),
    uow=Depends(get_unit_of_work)
):
    """
    Provides a UserService for user CRUD and profile operations.
    """
    return UserService(user_repo, uow)
