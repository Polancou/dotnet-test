"""
Database Seeder Script

Seeds the database with a default admin user if one does not already exist.
Intended for development, initial deployments, or automated testing.

This script checks for the presence of an admin user (username: 'admin'),
creating one with a default password if necessary. Additionally, error handling and
logging are provided to assist with debugging and operational visibility.

Usage:
    python seed.py
"""

from src.infrastructure.persistence.database import SessionLocal
from src.domain.entities.user import User
from src.domain.enums.user_role import UserRole
from src.infrastructure.services.services_impl import PasswordHasher

def seed_admin_user():
    """
    Checks if an admin user exists in the database.
    If not, creates one with default credentials and admin role.

    Handles session management, error output, and prints status updates.
    """
    db = SessionLocal()  # Create a new SQLAlchemy session for DB operations
    try:
        print("Checking for admin user...")

        # Query for existing user with username "admin"
        admin_user = db.query(User).filter(User.username == "admin").first()
        
        if not admin_user:
            print("Admin user not found. Creating...")

            # Initialize a password hasher for secure password storage
            hasher = PasswordHasher()
            password_hash = hasher.hash("Admin123!")  # Hash the default password
            
            # Create a new User instance with admin privileges
            new_admin = User(
                username="admin",
                email="admin@example.com",
                password_hash=password_hash,
                role=UserRole.ADMIN
            )
            
            db.add(new_admin)     # Add new admin user to session
            db.commit()           # Commit transaction to persist changes
            print("Admin user seeded successfully.")
        else:
            print("Admin user already exists.")
            
    except Exception as e:
        # Rollback any pending transaction on error and log exception
        print(f"Error seeding admin user: {e}")
        db.rollback()
    finally:
        # Always close the session to release DB connection
        db.close()

if __name__ == "__main__":
    # Run the seeding logic if this script is executed directly
    seed_admin_user()
