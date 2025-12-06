from pydantic import BaseModel, Field
from datetime import datetime
from src.domain.enums.user_role import UserRole

# -----------------------------------------------------------------------------
# Data Transfer Object (DTO) for User Representation
#
# This DTO is used for structuring user data as it is exchanged between
# backend services and API clients. It defines the fields made available for
# operations like listing users, displaying profiles, or administrative management.
# Field aliases are provided for JSON serialization style consistency.
# -----------------------------------------------------------------------------
class UserDto(BaseModel):
    """
    Represents a user in API responses.

    Attributes:
        id (int): Unique identifier for the user.
        username (str): The user's login name.
        email (str): The user's email address.
        role (UserRole): The user's role/privileges (e.g., admin, user).
        is_active (bool): Indicates if the user's account is currently active.
        creation_date (datetime): Account creation timestamp.
    """
    id: int  # Unique user identifier
    username: str  # Username or login handle
    email: str  # Registered email address
    role: UserRole  # Role or privilege level of the user
    is_active: bool = Field(serialization_alias="isActive")  # Is account active? Uses JSON alias isActive
    creation_date: datetime = Field(serialization_alias="creationDate")  # User creation date, JSON alias creationDate

    class Config:
        from_attributes = True  # Allow population from ORM or other object attributes
