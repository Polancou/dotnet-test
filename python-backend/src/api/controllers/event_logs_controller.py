"""
event_logs_controller.py

Defines endpoints for managing and retrieving event logs in the system.
Typically used for audit purposes, activity tracking, or history reporting.

Endpoints here are protected—only authenticated users can access event logs.
Returned logs may be filtered based on user identity and role (admin/user/etc.)
to provide appropriate access.
"""

from typing import List
from fastapi import APIRouter, Depends, HTTPException
from src.application.interfaces.interfaces import IEventLogService
from src.api.dependencies import get_event_log_service, get_current_user
from src.domain.entities.user import User
from src.domain.entities.event_log import EventLog

from src.application.dtos.event_log_dto import EventLogDto

# Create a FastAPI router with a URL prefix for event logs
router = APIRouter(prefix="/eventlogs", tags=["Event Logs"])

@router.get("/", response_model=List[EventLogDto])
def get_logs(
    event_log_service: IEventLogService = Depends(get_event_log_service),
    current_user: User = Depends(get_current_user)
):
    """
    Retrieve event logs for the current user.

    This endpoint fetches event logs from the event log service. The actual events
    returned are typically filtered based on the requesting user's ID and role.
    Admins may view all logs, while normal users may only see their own events.

    Args:
        event_log_service (IEventLogService): Dependency-injected event log service.
        current_user (User): The authenticated user making the request.

    Returns:
        list: A list of event log objects relevant to the user.

    Raises:
        HTTPException: Propagates exceptions if the underlying service fails.
    """
    # Note: In the .NET reference implementation, filtering by user and/or role 
    # occurs inside the service/repository layer. The method signature here mirrors that.
    # Make sure get_logs is implemented in the event log service with (user_id, user_role) params.
    return event_log_service.get_logs(current_user.id, current_user.role)
