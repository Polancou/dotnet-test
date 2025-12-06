"""
create_db.py

Script to ensure that the 'CleanArchDb' SQL Server database exists.
- Connects to the SQL Server instance using pymssql.
- Checks if the target database exists.
- If not present, creates the database.
- Designed for container/docker entrypoint usage (not for general application import).
"""

import os
import time
import pymssql

def create_database():
    """
    Ensures that the 'CleanArchDb' database exists on the target SQL Server.
    Attempts connection using environment variables for credentials.
    """
    # Target SQL Server instance and admin user
    server = "sqlserver"
    user = "sa"
    # Attempt to get the SA password from env vars (DB_PASSWORD preferred)
    password = os.getenv("DB_PASSWORD") or os.getenv("MSSQL_SA_PASSWORD")
    
    # If password is still not found, try to parse from DATABASE_URL (if possible)
    if not password:
        db_url = os.getenv("DATABASE_URL")
        if db_url and ":1433" in db_url:
            try:
                # Example format: mssql+pymssql://sa:PASSWORD@sqlserver:1433/CleanArchDb
                # Split to get the credentials portion, then extract the password
                part1 = db_url.split("@")[0]
                # mssql+pymssql://sa:PASSWORD
                password = part1.split(":")[2]
            except Exception:
                print("Could not extract password from DATABASE_URL")
    
    # Fail if password is still not available
    if not password:
        print("Error: DB_PASSWORD environment variable not set.")
        return

    print(f"Connecting to SQL Server '{server}' as user '{user}'...")

    conn = None
    retries = 5
    # Retry logic for the initial connection (handles DB not ready yet)
    while retries > 0:
        try:
            # Connect to the 'master' DB to perform admin operations
            conn = pymssql.connect(
                server=server,
                user=user,
                password=password,
                database="master",
                autocommit=True
            )
            break  # Connection successful
        except Exception as e:
            print(f"Connection failed: {e}. Retrying in 2 seconds...")
            time.sleep(2)
            retries -= 1

    # Exit if connection was unsuccessful after retries
    if not conn:
        print("Could not connect to SQL Server.")
        exit(1)

    cursor = conn.cursor()
    
    try:
        # Check for the existence of the target database
        cursor.execute("SELECT name FROM master.dbo.sysdatabases WHERE name = N'CleanArchDb'")
        row = cursor.fetchone()
        if row:
            print("Database 'CleanArchDb' already exists.")
        else:
            print("Database 'CleanArchDb' does not exist. Creating...")
            cursor.execute("CREATE DATABASE CleanArchDb")
            print("Database 'CleanArchDb' created successfully.")
    except Exception as e:
        print(f"Error creating database: {e}")
        exit(1)
    finally:
        # Always close the connection
        conn.close()

if __name__ == "__main__":
    # Entrypoint: execute database creation logic when run as a script
    create_database()
