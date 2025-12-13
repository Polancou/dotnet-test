from pydantic import BaseModel, Field
from datetime import datetime
from typing import Optional

class EventLogDto(BaseModel):
    """
    Data Transfer Object for EventLog.
    Ensures consistent JSON serialization (camelCase).
    """
    id: int
    event_type: str = Field(serialization_alias="eventType")
    description: str
    user_id: Optional[int] = Field(default=None, serialization_alias="userId")
    creation_date: datetime = Field(serialization_alias="creationDate")

    class Config:
        from_attributes = True
