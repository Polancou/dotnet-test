"""
BaseEntity module defines the foundational SQLAlchemy model for all entities.

This abstract base class provides:
- An integer primary key 'id'
- Soft deletion toggle 'is_active'
- Automatic audit timestamps ('creation_date', 'modification_date')
- Utility methods for activation/deactivation and audit updates
"""

from datetime import datetime
from sqlalchemy import Column, Integer, Boolean, DateTime
from sqlalchemy.orm import declarative_base

# SQLAlchemy declarative base for ORM mapping.
Base = declarative_base()

class BaseEntity(Base):
    """
    Abstract base class for all ORM entities, providing common attributes and methods.

    Fields:
        id (int): Primary key, auto-incrementing.
        is_active (bool): Entity's soft-delete/active state.
        creation_date (datetime): UTC timestamp of creation.
        modification_date (datetime): UTC timestamp of last modification.
    """

    __abstract__ = True  # This class will not be mapped to a table directly.

    # Unique identifier (primary key) for each entity instance
    id = Column(Integer, primary_key=True, autoincrement=True)

    # Soft deletion / state flag
    is_active = Column(Boolean, default=True, nullable=False)

    # Creation timestamp, set on insertion
    creation_date = Column(DateTime, default=datetime.utcnow, nullable=False)

    # Last modification timestamp, updated via update_modification_date()
    modification_date = Column(DateTime, nullable=True)

    def update_modification_date(self):
        """
        Updates the modification_date to the current UTC time.
        Should be called whenever the entity is modified.
        """
        self.modification_date = datetime.utcnow()

    def deactivate(self):
        """
        Soft-deactivate this entity (marks as inactive).
        Sets is_active to False and updates modification_date.
        """
        self.is_active = False
        self.update_modification_date()

    def activate(self):
        """
        Reactivate this entity (marks as active).
        Sets is_active to True and updates modification_date.
        """
        self.is_active = True
        self.update_modification_date()
