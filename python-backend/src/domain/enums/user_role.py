from enum import Enum

class UserRole(str, Enum):
    """
    UserRole enumeration defines all possible roles assignable to users in the system.

    The values are:
    - ADMIN: Has full administrative privileges.
    - USER: Regular user with standard access.
    """

    ADMIN = "Admin"  # Full administrative privileges (manage all aspects of the system)
    USER = "User"    # Basic user privileges (restricted/non-admin tasks)
