"""
Structure and DTO integrity tests for core domain entities.

This script verifies basic instantiation and member correctness for the User entity
and the UserDto data transfer object. These are not integration or business logic tests,
but "smoke tests" for the constructors and enum usage.
"""

import sys
import os

# Ensure the project root is in the Python path so local imports work during test execution.
sys.path.append(os.path.abspath(os.path.join(os.path.dirname(__file__), '..')))

from src.domain.entities.user import User
from src.domain.enums.user_role import UserRole
from src.application.dtos.user_dto import UserDto
from datetime import datetime

def test_user_creation():
    """
    Test the creation of a User object with all required parameters and
    validate that its attributes are set correctly.
    """
    # Create a user with dummy data and role USER
    user = User("testuser", "test@example.com", "hashed_password", UserRole.USER)
    # Check all expected field assignments
    assert user.username == "testuser"
    assert user.email == "test@example.com"
    assert user.role == UserRole.USER
    assert user.is_active is True  # Default should be True for new users
    print("User creation test passed")

def test_user_dto_creation():
    """
    Test the instantiation of a UserDto, ensuring it reflects DTO properties and preserves type.
    """
    # Creation date is set to now for testing
    now = datetime.utcnow()
    user_dto = UserDto(
        id=1,
        username="testuser",
        email="test@example.com",
        role=UserRole.USER,
        is_active=True,
        creation_date=now
    )
    # Only basic attribute checks; detailed field tests elsewhere if needed
    assert user_dto.username == "testuser"
    assert user_dto.role == UserRole.USER
    assert user_dto.creation_date == now
    print("UserDto creation test passed")

if __name__ == "__main__":
    # Run structure smoke tests
    test_user_creation()
    test_user_dto_creation()
