from typing import List, Optional, Type
from sqlalchemy.orm import Session
from src.application.interfaces.interfaces import (
    IGenericRepository,
    IUserRepository,
    IDocumentRepository,
    IEventLogRepository,
)
from src.domain.common.base_entity import BaseEntity
from src.domain.entities.user import User
from src.domain.entities.document import Document
from src.domain.entities.event_log import EventLog

# /////////////////////////////////////////////////////////////////////////////
# Generic Repository pattern implementation for SQLAlchemy models.
# Provides universal CRUD operations for any entity derived from BaseEntity.
# /////////////////////////////////////////////////////////////////////////////
class GenericRepository(IGenericRepository[BaseEntity]):
    def __init__(self, session: Session, model_cls: Type[BaseEntity]):
        """
        Initialize the generic repository with the session and model class.
        Args:
            session (Session): SQLAlchemy session for database interactions.
            model_cls (Type[BaseEntity]): ORM mapped entity class.
        """
        self.session = session
        self.model_cls = model_cls

    def get_by_id(self, id: int) -> Optional[BaseEntity]:
        """
        Fetch an entity by its primary key.
        Args:
            id (int): The ID of the entity.
        Returns:
            Optional[BaseEntity]: The entity instance if found, else None.
        """
        return self.session.query(self.model_cls).filter(self.model_cls.id == id).first()

    def get_all(self) -> List[BaseEntity]:
        """
        Fetch all entities of this model class.
        Returns:
            List[BaseEntity]: List of all entity instances.
        """
        return self.session.query(self.model_cls).all()

    def add(self, entity: BaseEntity):
        """
        Add a new entity instance to the session.
        Args:
            entity (BaseEntity): Entity to be added.
        """
        self.session.add(entity)

    def update(self, entity: BaseEntity):
        """
        Mark an entity as updated for the session.
        In SQLAlchemy, tracked objects are auto-updated,
        but this ensures the session knows about changes if necessary.
        Args:
            entity (BaseEntity): The entity to update.
        """
        # In SQLAlchemy, adding an attached instance is a no-op but is safe to ensure tracking for updates.
        self.session.add(entity)

    def remove(self, entity: BaseEntity):
        """
        Remove (delete) an entity from the session.
        Args:
            entity (BaseEntity): The entity to remove.
        """
        self.session.delete(entity)

# /////////////////////////////////////////////////////////////////////////////
# UserRepository: Domain-specific repository for User entities.
# Provides user-centric queries (by username, email, refresh token).
# /////////////////////////////////////////////////////////////////////////////
class UserRepository(GenericRepository, IUserRepository):
    def __init__(self, session: Session):
        """
        Initialize with SQLAlchemy session, bound to User model.
        Args:
            session (Session): SQLAlchemy session.
        """
        super().__init__(session, User)

    def get_by_username(self, username: str) -> Optional[User]:
        """
        Fetch a user by their username.
        Args:
            username (str): The user's username.
        Returns:
            Optional[User]: User instance if found, else None.
        """
        return self.session.query(User).filter(User.username == username).first()

    def get_by_email(self, email: str) -> Optional[User]:
        """
        Fetch a user by their email address.
        Args:
            email (str): The user's email.
        Returns:
            Optional[User]: User instance if found, else None.
        """
        return self.session.query(User).filter(User.email == email).first()

    def get_by_refresh_token(self, refresh_token: str) -> Optional[User]:
        """
        Fetch a user by their refresh token.
        Args:
            refresh_token (str): Refresh token string.
        Returns:
            Optional[User]: User instance if found, else None.
        """
        return self.session.query(User).filter(User.refresh_token == refresh_token).first()

# /////////////////////////////////////////////////////////////////////////////
# DocumentRepository: Domain-specific repository for Document entities.
# Provides queries related to documents (e.g. by owner/user ID).
# /////////////////////////////////////////////////////////////////////////////
class DocumentRepository(GenericRepository, IDocumentRepository):
    def __init__(self, session: Session):
        """
        Initialize with SQLAlchemy session, bound to Document model.
        Args:
            session (Session): SQLAlchemy session.
        """
        super().__init__(session, Document)

    def get_by_user_id(self, user_id: int) -> List[Document]:
        """
        Fetch all documents uploaded by a specific user.
        Args:
            user_id (int): The user ID.
        Returns:
            List[Document]: List of Document instances uploaded by given user.
        """
        return self.session.query(Document).filter(Document.uploaded_by_user_id == user_id).all()

# /////////////////////////////////////////////////////////////////////////////
# EventLogRepository: Domain-specific repository for EventLog entities.
# For audit logging and event tracking.
# /////////////////////////////////////////////////////////////////////////////
class EventLogRepository(GenericRepository, IEventLogRepository):
    def __init__(self, session: Session):
        """
        Initialize with SQLAlchemy session, bound to EventLog model.
        Args:
            session (Session): SQLAlchemy session.
        """
        super().__init__(session, EventLog)
