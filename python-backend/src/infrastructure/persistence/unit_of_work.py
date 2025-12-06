from sqlalchemy.orm import Session
from src.application.interfaces.interfaces import IUnitOfWork

class UnitOfWork(IUnitOfWork):
    """
    UnitOfWork implementation for SQLAlchemy sessions.

    Manages a database session lifecycle, including committing and rolling back
    transactions as a single atomic operation for a business process.
    Implements the IUnitOfWork interface to enable dependency inversion for testing.
    """

    def __init__(self, session: Session):
        """
        Initialize the UnitOfWork with an existing SQLAlchemy session.

        Args:
            session (Session): The SQLAlchemy session object for database operations.
        """
        self.session = session

    def commit(self):
        """
        Commit the current transaction.

        Persists all pending changes in the current session to the database.
        """
        self.session.commit()

    def rollback(self):
        """
        Roll back the current transaction.

        Undoes any changes made in the session that have not been committed,
        effectively reverting the session to its original state.
        """
        self.session.rollback()
