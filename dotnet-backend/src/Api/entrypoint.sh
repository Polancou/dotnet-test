#!/bin/bash
set -e

# Function to wait for SQL Server
wait_for_sql() {
    echo "Waiting for SQL Server..."
    # Using nc (netcat) to check port
    until nc -z -v -w30 sqlserver 1433; do
        echo "SQL Server is unavailable - sleeping"
        sleep 1
    done
    echo "SQL Server is up!"
}

wait_for_sql

echo "Starting application..."
exec "$@"
