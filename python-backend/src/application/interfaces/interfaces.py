from abc import ABC, abstractmethod
from typing import TypeVar, Generic, List, Optional, Any

from src.domain.common.base_entity import BaseEntity
from src.domain.entities.user import User
from src.domain.entities.document import Document
from src.domain.entities.event_log import EventLog
from src.domain.enums.user_role import UserRole
from src.application.dtos.auth_dtos import LoginRequest, LoginResponse, RegisterRequest
from src.application.dtos.user_dto import UserDto

T = TypeVar("T", bound=BaseEntity)

# ------------------------------------------------------------------------------
# Generic repository interface. Provides standard CRUD operations contract
# for entities. Actual repositories (User, Document, etc.) inherit from this.
# ------------------------------------------------------------------------------
class IGenericRepository(ABC, Generic[T]):
    @abstractmethod
    def get_by_id(self, id: int) -> Optional[T]:
        """
        Retrieve an entity by its primary key ID.
        """
        pass

    @abstractmethod
    def get_all(self) -> List[T]:
        """
        Retrieve all entities of type T.
        """
        pass

    @abstractmethod
    def add(self, entity: T):
        """
        Add a new entity to the store.
        """
        pass

    @abstractmethod
    def update(self, entity: T):
        """
        Update an existing entity.
        """
        pass

    @abstractmethod
    def remove(self, entity: T):
        """
        Remove (delete) an entity.
        """
        pass

# ------------------------------------------------------------------------------
# Specialized repository interface for User entity.
# Adds methods specific to user lookups and credentials.
# ------------------------------------------------------------------------------
class IUserRepository(IGenericRepository[User]):
    @abstractmethod
    def get_by_username(self, username: str) -> Optional[User]:
        """
        Retrieve a user entity by its username.
        """
        pass

    @abstractmethod
    def get_by_email(self, email: str) -> Optional[User]:
        """
        Retrieve a user entity by email address.
        """
        pass

    @abstractmethod
    def get_by_refresh_token(self, refresh_token: str) -> Optional[User]:
        """
        Retrieve a user entity associated with a refresh token.
        """
        pass

# ------------------------------------------------------------------------------
# Specialized repository interface for Document entity.
# ------------------------------------------------------------------------------
class IDocumentRepository(IGenericRepository[Document]):
    @abstractmethod
    def get_by_user_id(self, user_id: int) -> List[Document]:
        """
        Retrieve all documents belonging to a given user.
        """
        pass

# ------------------------------------------------------------------------------
# Event log repository interface. Just the generic CRUD for event logs.
# ------------------------------------------------------------------------------
class IEventLogRepository(IGenericRepository[EventLog]):
    """
    Repository contract for event log entities.
    """
    pass

# ------------------------------------------------------------------------------
# Unit of Work interface for handling transactions across multiple repositories.
# ------------------------------------------------------------------------------
class IUnitOfWork(ABC):
    @abstractmethod
    def commit(self):
        """
        Commit all pending changes as a single transaction.
        """
        pass

    @abstractmethod
    def rollback(self):
        """
        Rollback all pending changes.
        """
        pass

# ------------------------------------------------------------------------------
# Interface for password hashing and verification.
# ------------------------------------------------------------------------------
class IPasswordHasher(ABC):
    @abstractmethod
    def hash(self, password: str) -> str:
        """
        Hash a plaintext password to store securely.
        """
        pass

    @abstractmethod
    def verify(self, password: str, password_hash: str) -> bool:
        """
        Verify a password against its hash.
        """
        pass

# ------------------------------------------------------------------------------
# Interface for JWT token generation.
# ------------------------------------------------------------------------------
class IJwtService(ABC):
    @abstractmethod
    def generate_access_token(self, user: User) -> str:
        """
        Generate a JWT access token for the given user.
        """
        pass

    @abstractmethod
    def generate_refresh_token(self) -> str:
        """
        Generate a secure refresh token.
        """
        pass

# ------------------------------------------------------------------------------
# Service interface for event logging functionality.
# ------------------------------------------------------------------------------
class IEventLogService(ABC):
    @abstractmethod
    def log_event(self, event_type: str, description: str, user_id: Optional[int] = None):
        """
        Log an event to the system with optional user association.
        """
        pass

    @abstractmethod
    def get_logs(self, user_id: int, role: UserRole) -> List[EventLog]:
        """
        Retrieve event logs for a user, filtered by user and role.
        """
        pass

# ------------------------------------------------------------------------------
# Authentication business logic interface.
# ------------------------------------------------------------------------------
class IAuthService(ABC):
    @abstractmethod
    def login(self, request: LoginRequest) -> LoginResponse:
        """
        Authenticate a user and return an access/refresh token pair.
        """
        pass

    @abstractmethod
    def register(self, request: RegisterRequest):
        """
        Register a new user.
        """
        pass

    @abstractmethod
    def refresh_token(self, token: str) -> LoginResponse:
        """
        Refresh the access token using a valid refresh token.
        """
        pass

# ------------------------------------------------------------------------------
# Service interface for user management operations.
# ------------------------------------------------------------------------------
class IUserService(ABC):
    @abstractmethod
    def get_all_users(self) -> List[UserDto]:
        """
        Retrieve a list of all users in DTO form.
        """
        pass

    @abstractmethod
    def update_user_role(self, user_id: int, new_role: UserRole):
        """
        Update a user's role (e.g., promote/demote).
        """
        pass

# ------------------------------------------------------------------------------
# Interface for file storage; used to upload and save files (e.g., documents).
# ------------------------------------------------------------------------------
class IFileStorageService(ABC):
    @abstractmethod
    def save_file(self, file_content: bytes, file_name: str) -> str:
        """
        Save a file with content and filename; returns the storage location or URL.
        """
        pass

# ------------------------------------------------------------------------------
# Interface for bulk CSV user upload processing.
# ------------------------------------------------------------------------------
class ICsvService(ABC):
    @abstractmethod
    def process_user_bulk_upload(self, csv_content: bytes) -> Any:
        """
        Process a bulk user upload from CSV format.
        Returns summary, report, or result as needed.
        """
        pass

# ------------------------------------------------------------------------------
# Service interface for document operations: upload, listing, update of analyses.
# ------------------------------------------------------------------------------
class IDocumentService(ABC):
    @abstractmethod
    def upload_document(self, file_name: str, content: bytes, content_type: str,
                        user_id: int, role: UserRole, process_type: Optional[str] = None) -> Any:
        """
        Store a document and start processing if needed.
        """
        pass

    @abstractmethod
    def get_user_documents(self, user_id: int) -> List[Document]:
        """
        Retrieve all documents for a specific user.
        """
        pass

    @abstractmethod
    def update_analysis_result(self, document_id: int, analysis_result: str):
        """
        Save or update the analysis/parsing result for a document.
        """
        pass

# ------------------------------------------------------------------------------
# Interface for running AI-based document analysis.
# ------------------------------------------------------------------------------
class IAiAnalysisService(ABC):
    @abstractmethod
    def analyze_document(self, file_content: bytes, file_name: str) -> Any:
        """
        Analyze a document using AI/ML models and return results.
        """
        pass
