"""
This module sets up the SQLAlchemy database engine, session factory, and utility functions
for interacting with the application's relational database.

- Configures the database connection using the DATABASE_URL from settings.
- Provides a dependency function (`get_db`) to yield a database session with proper cleanup.
- Defines a function to create all tables according to the project's ORM mappings.

Intended for use across the infrastructure and repository layers.
"""

from sqlalchemy import create_engine
from sqlalchemy.orm import sessionmaker, Session
from src.domain.common.base_entity import Base
from src.config import settings

# Retrieve the database URL from application settings/configuration.
# Example: "sqlite:///./test.db" or any other SQLAlchemy-supported URL.
SQLALCHEMY_DATABASE_URL = settings.DATABASE_URL

# Define additional connection arguments if using SQLite.
# The 'check_same_thread' parameter is required for SQLite database access across threads.
connect_args = {}
if "sqlite" in SQLALCHEMY_DATABASE_URL:
    connect_args = {"check_same_thread": False}

# Create the SQLAlchemy engine, which manages the connection pool and DBAPI interface.
engine = create_engine(
    SQLALCHEMY_DATABASE_URL, connect_args=connect_args
)

# Configure a session factory bound to the engine.
# - autocommit=False: Transactions are managed manually.
# - autoflush=False: Prevents changes from being flushed to the database automatically.
SessionLocal = sessionmaker(autocommit=False, autoflush=False, bind=engine)

def get_db():
    """
    Dependency generator to create and clean up a database session.
    Yields:
        Session: The SQLAlchemy database session.
    Ensures:
        The session is closed after use, even if exceptions occur.
    """
    db = SessionLocal()
    try:
        yield db
    finally:
        db.close()

def create_tables():
    """
    Create all ORM-mapped tables in the database.

    This function uses SQLAlchemy Base metadata to create tables for all defined
    entity models. Should be called during application startup or migrations.
    """
    Base.metadata.create_all(bind=engine)
