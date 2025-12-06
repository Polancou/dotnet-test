#!/bin/bash
set -e

################################################################################
# Entrypoint Script for Python Backend (Docker/Production)
#
# Responsibilities:
#   - Waits for SQL Server to become available before proceeding.
#   - Ensures the target database exists (using create_db.py).
#   - Applies latest migrations with Alembic.
#   - Seeds an admin user if none exists.
#   - Finally, executes the given command (usually starts the app server).
################################################################################

# Function: wait_for_sql
# Purpose : Polls the SQL Server instance until it is accepting connections.
#           Prevents the app from starting up before the database is ready.
wait_for_sql() {
    echo "Waiting for SQL Server..."
    max_attempts=60
    attempt=0
    # Loop until TCP connection to 'sqlserver:1433' is successful
    while [ $attempt -lt $max_attempts ]; do
        if python -c "import socket; s = socket.socket(socket.AF_INET, socket.SOCK_STREAM); s.settimeout(2); result = s.connect_ex(('sqlserver', 1433)); s.close(); exit(0 if result == 0 else 1)" 2>/dev/null; then
            echo "SQL Server is up!"
            # Give SQL Server a few more seconds to fully initialize
            echo "Waiting additional 5 seconds for SQL Server to fully initialize..."
            sleep 5
            return 0
        fi
        attempt=$((attempt + 1))
        echo "SQL Server is unavailable - attempt $attempt/$max_attempts - sleeping"
        sleep 2
    done
    echo "ERROR: SQL Server did not become available after $max_attempts attempts"
    exit 1
}

# Step 1: Wait for SQL Server to be accepting connections
wait_for_sql

# Step 2: Ensure the database exists (if not, create it)
echo "Ensuring database exists..."
python create_db.py

# Step 3: Apply all new alembic database migrations
echo "Running migrations..."
alembic upgrade head

# Step 4: Seed the admin user if needed
echo "Seeding admin user..."
python seed.py

# Step 5: Start the actual application, passing any CMD arguments from Dockerfile
echo "Starting application..."
exec "$@"
