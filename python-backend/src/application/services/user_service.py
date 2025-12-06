from typing import List
from src.application.interfaces.interfaces import IUserService, IUserRepository, IUnitOfWork
from src.application.dtos.user_dto import UserDto
from src.domain.enums.user_role import UserRole

class UserService(IUserService):
    """
    Service responsible for user-related business logic operations,
    including retrieval and updates of user information.
    """

    def __init__(self, user_repository: IUserRepository, unit_of_work: IUnitOfWork):
        """
        Initializes the UserService with required dependencies for
        user data access and transactional operations.

        Args:
            user_repository (IUserRepository): Repository for accessing user data.
            unit_of_work (IUnitOfWork): Handles database commits and rollbacks.
        """
        self.user_repository = user_repository
        self.unit_of_work = unit_of_work

    def get_all_users(self) -> List[UserDto]:
        """
        Retrieves all users from the repository and converts them into DTOs.

        Returns:
            List[UserDto]: A list of user data transfer objects representing all users.
        """
        users = self.user_repository.get_all()  # Fetch all user entities
        # Convert each user entity to a DTO using from_orm factory function
        return [UserDto.from_orm(user) for user in users]

    def update_user_role(self, user_id: int, new_role: UserRole):
        """
        Updates the role of a specific user.

        Args:
            user_id (int): Unique identifier of the user to update.
            new_role (UserRole): The new role to assign to the user.

        Raises:
            Exception: If the user does not exist.
        """
        user = self.user_repository.get_by_id(user_id)  # Retrieve user by ID
        if not user:
            # Raise an exception if user cannot be found
            raise Exception("User not found")
        
        user.update_role(new_role)         # Update user's role using domain entity method
        self.unit_of_work.commit()         # Persist changes to the database transactionally
