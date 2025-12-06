"""
users_controller.py

Defines API endpoints related to user management.

Currently exposes a single endpoint to retrieve the list of all users.
This controller is protected—only authenticated users can access it.
"""

from typing import List
from fastapi import APIRouter, Depends
from src.application.interfaces.interfaces import IUserService
from src.application.dtos.user_dto import UserDto
from src.api.dependencies import get_user_service, get_current_user

# Create an API router for user-related routes, with the prefix "/users"
router = APIRouter(prefix="/users", tags=["Users"])

@router.get("/", response_model=List[UserDto])
def get_all_users(
    user_service: IUserService = Depends(get_user_service),
    current_user = Depends(get_current_user)
):
    """
    Retrieve a list of all users in the system.

    Only accessible to authenticated users. The underlying user service handles
    access control and filtering (if necessary, e.g., for admin-only operations).

    Args:
        user_service (IUserService): Injected service for user operations.
        current_user: The currently authenticated user (used for authorization).

    Returns:
        List[UserDto]: A list of users represented as DTOs.
    """
    # Fetch the list of users from the service layer.
    # The service is responsible for enforcing any authorization policies.
    return user_service.get_all_users()
