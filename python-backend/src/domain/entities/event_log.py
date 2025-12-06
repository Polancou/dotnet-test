from sqlalchemy import Column, String, Integer, ForeignKey
from sqlalchemy.orm import relationship
from src.domain.common.base_entity import BaseEntity

class EventLog(BaseEntity):
    """
    EventLog entity/model representing an event or action in the system.

    Inherits from BaseEntity, providing:
    - Standard id, is_active, and audit timestamps.

    Captures:
    - event_type: Type or category of the event (e.g., 'LOGIN', 'UPLOAD_DOCUMENT')
    - description: Description or additional details of the event
    - user_id: (Optional) Foreign key to the associated user (can be null if the event is system-generated)
    - user: SQLAlchemy relationship to the User entity

    Relationships:
    - user: Reference to the User entity responsible for or associated with the event.

    Usage:
        Used for audit logging, security, and tracking important actions within the application.
    """
    __tablename__ = "event_logs"

    # Type/category of the event, required (example: 'LOGIN', 'FILE_UPLOAD')
    event_type = Column(String, nullable=False)

    # Description of the event, required (example: 'User uploaded a file')
    description = Column(String, nullable=False)

    # Optional - ID of the user associated with the event, may be null for system events
    user_id = Column(Integer, ForeignKey("users.id"), nullable=True)

    # SQLAlchemy relationship to the User entity (bi-directional if configured)
    user = relationship("User")

    def __init__(self, event_type: str, description: str, user_id: int = None):
        """
        Initialize a new EventLog entity with event details and optional user.

        Args:
            event_type (str): The type/category of the event. Cannot be empty.
            description (str): Detailed description of the event. Cannot be empty.
            user_id (int, optional): Foreign key of the associated user; can be None.

        Raises:
            ValueError: If event_type or description is empty.
        """
        # Validate that event_type is provided
        if not event_type:
            raise ValueError("Event type cannot be empty")
        # Validate that description is provided
        if not description:
            raise ValueError("Description cannot be empty")

        # Set entity fields from provided arguments
        self.event_type = event_type
        self.description = description
        self.user_id = user_id

        # Call BaseEntity initializer, defaulting to active record
        super().__init__(is_active=True)
