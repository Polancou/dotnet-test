import io
import csv
from typing import Any
from src.application.interfaces.interfaces import ICsvService, IUserRepository, IUnitOfWork, IPasswordHasher
from src.domain.entities.user import User
from src.domain.enums.user_role import UserRole

class CsvService(ICsvService):
    """
    Service for processing user bulk uploads from CSV files.

    This class implements logic to handle reading, validating, and importing users
    in bulk from CSV content. It ensures correctness, handles duplicate or invalid
    entries, and persists new users in a transactional fashion.
    """

    def __init__(self, user_repository: IUserRepository, unit_of_work: IUnitOfWork, password_hasher: IPasswordHasher):
        """
        Constructor for CsvService.

        Args:
            user_repository (IUserRepository): Repository to interact with User data.
            unit_of_work (IUnitOfWork): Unit of work for database commits/transactions.
            password_hasher (IPasswordHasher): Service for hashing passwords.
        """
        self.user_repository = user_repository
        self.unit_of_work = unit_of_work
        self.password_hasher = password_hasher

    def process_user_bulk_upload(self, csv_content: bytes) -> Any:
        """
        Processes a CSV file containing user data for bulk upload.

        The expected CSV format is: Username, Email, Password, Role
        - Skips the header row (assumes it exists).
        - Each user is validated:
            * Checks for minimum required columns.
            * Validates presence of username, email, and password.
            * Resolves user role case-insensitively.
            * Checks for duplicate usernames.
            * Hashes passwords before saving.
        - Aggregates counts and errors for successes and failures.

        Args:
            csv_content (bytes): The byte contents of the CSV file.

        Returns:
            dict: {
                "success_count": int,    # Number of users successfully added.
                "failure_count": int,    # Number of failures.
                "errors": List[str],     # List of error descriptions (with line reference).
            }
        """
        success_count = 0  # Number of users processed successfully.
        failure_count = 0  # Number of users that failed to process.
        errors = []        # List collecting error messages with line numbers.

        # Decode bytes to UTF-8 string, prepare in-memory file object, and create a CSV reader.
        content_str = csv_content.decode('utf-8')
        csv_file = io.StringIO(content_str)
        reader = csv.reader(csv_file)

        # Skip the CSV header row.
        next(reader, None)

        line_number = 1  # Tracks the current line (starts from second, after header).
        for row in reader:
            line_number += 1
            # Skip empty rows.
            if not row:
                continue

            # Validate row has at least 4 columns (username, email, password, role).
            if len(row) < 4:
                failure_count += 1
                errors.append(f"Line {line_number}: Invalid format. Expected Username,Email,Password,Role")
                continue

            # Extract user info, stripping extraneous whitespace.
            username = row[0].strip()
            email = row[1].strip()
            password = row[2].strip()
            role_str = row[3].strip()

            # Ensure required fields are present.
            if not username or not email or not password:
                failure_count += 1
                errors.append(f"Line {line_number}: Missing required fields.")
                continue

            # Attempt to resolve the role, matching case-insensitively against all possible UserRole values.
            try:
                # Try to match using enum values (string match)
                role = next((r for r in UserRole if r.value.lower() == role_str.lower()), None)
                if not role:
                    # Provide fallback for common roles (if e.g. input is not standard).
                    if role_str.lower() == "admin":
                        role = UserRole.ADMIN
                    elif role_str.lower() == "user":
                        role = UserRole.USER
                    else:
                        failure_count += 1
                        errors.append(f"Line {line_number}: Invalid role '{role_str}'.")
                        continue
            except ValueError:
                # Should only trigger if UserRole is misconfigured.
                failure_count += 1
                errors.append(f"Line {line_number}: Invalid role '{role_str}'.")
                continue

            # Check if a user with the username already exists.
            existing_user = self.user_repository.get_by_username(username)
            if existing_user:
                failure_count += 1
                errors.append(f"Line {line_number}: User '{username}' already exists.")
                continue

            # Try creating and adding the user.
            try:
                password_hash = self.password_hasher.hash(password)
                user = User(username, email, password_hash, role)
                self.user_repository.add(user)
                success_count += 1
            except Exception as ex:
                failure_count += 1
                errors.append(f"Line {line_number}: Error creating user. {str(ex)}")

        # Only commit changes if there was at least one success.
        if success_count > 0:
            self.unit_of_work.commit()

        # Provide a summary of processing.
        return {
            "success_count": success_count,
            "failure_count": failure_count,
            "errors": errors
        }
