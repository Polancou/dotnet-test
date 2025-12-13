from typing import Optional, List
from src.application.interfaces.interfaces import IEventLogService, IEventLogRepository, IUnitOfWork
from src.domain.entities.event_log import EventLog
from src.domain.enums.user_role import UserRole

class EventLogService(IEventLogService):
    """
    Service class responsible for handling all event log operations,
    such as recording new events and retrieving logs for auditing or display.

    This service acts as the bridge between the domain logic and
    persistence layers for event log activities.
    """

    def __init__(self, event_log_repository: IEventLogRepository, unit_of_work: IUnitOfWork):
        """
        Initializes the EventLogService with repository and transaction management.

        Args:
            event_log_repository (IEventLogRepository): Handles persistence of EventLog entities.
            unit_of_work (IUnitOfWork): Responsible for committing/rolling back transactions.
        """
        self.event_log_repository = event_log_repository
        self.unit_of_work = unit_of_work

    async def log_event(self, event_type: str, description: str, user_id: Optional[int] = None):
        """
        Records a new event in the event log.

        Args:
            event_type (str): The type/category of the event (e.g., 'Login', 'DocumentUpload').
            description (str): Textual description with extra event info.
            user_id (Optional[int]): The ID of the user who caused the event (if any).

        Effects:
            - Persists the event log entry via the repository.
            - Immediately commits the transaction.
            - Broadcasts the event to connected clients via WebSocket.
        """
        log = EventLog(event_type, description, user_id)  # Create the EventLog domain entity
        self.event_log_repository.add(log)                # Persist the log entry
        self.unit_of_work.commit()                        # Commit the operation to the database
        
        # Broadcast via connection manager if available
        from src.infrastructure.realtime.connection_manager import connection_manager
        
        try:
            # Create DTO-like dict for frontend
            log_dto = {
                "type": "ReceiveLog",
                "payload": {
                    "id": log.id,
                    "eventType": log.event_type,
                    "description": log.description,
                    "userId": log.user_id,
                    "creationDate": log.creation_date.isoformat() if log.creation_date else None
                }
            }
            await connection_manager.broadcast(log_dto)
        except Exception as e:
            print(f"Failed to broadcast event: {e}")

    def get_logs(self, user_id: int, role: UserRole) -> List[EventLog]:
        """
        Retrieves event logs, filtered by user and role.

        Admin users receive all event logs; non-admins only receive their own logs.
        (For MVP, this is in-memory, but in production should be implemented as a
        repository DB-query for scalability.)

        Args:
            user_id (int): The requesting user's ID (used for non-admin filtering).
            role (UserRole): The role of the requesting user, determines permissions.

        Returns:
            List[EventLog]: The list of event logs available to the user.
        """
        all_logs = self.event_log_repository.get_all()  # Retrieve all event logs from storage

        if role.value == "Admin":
            # Admin users are allowed to see all logs in the system
            return all_logs
        else:
            # Regular users only see their own logs (by user_id)
            return [log for log in all_logs if log.user_id == user_id]
