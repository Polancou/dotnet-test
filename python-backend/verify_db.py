"""
verify_db.py

A simple debugging script for verifying SQL Server database connectivity and
connection string parsing during development or troubleshooting.

This script does two things:
  1. Attempts to connect directly to SQL Server using hardcoded credentials via pymssql.
  2. Parses and validates the password from the application's DATABASE_URL config.

Usage:
    python verify_db.py

Author: (your name/team)
"""

import pymssql
from src.config import settings
import sys
from urllib.parse import urlparse, unquote

print("--- Debugging Connection ---")

# --- Test #1: Direct SQL connection using hardcoded credentials ---
print("\n1. Testing direct pymssql connection...")

try:
    # Attempt to establish a connection to the local SQL Server instance
    # Credentials must match the test/development container or DB
    conn = pymssql.connect(
        server='localhost',   # SQL Server host (default Docker mapped)
        user='sa',            # SQL Server system administrator account
        password='P@ssw0rd',  # Replace with your test/dev password as appropriate
        database='CleanArchDb',
        port=1433             # Default SQL Server port
    )
    print("   SUCCESS: Direct pymssql connection worked!")
    conn.close()  # Close the connection if successful
except Exception as e:
    # Print the exception for diagnosis if connection fails (common causes: network/firewall, wrong credentials, DB not started)
    print(f"   FAILURE: Direct pymssql connection failed: {e}")

# --- Test #2: DATABASE_URL parsing and password decoding ---
print("\n2. Testing DATABASE_URL parsing...")

# Retrieve the DATABASE_URL from application settings
url = settings.DATABASE_URL
# Obscure password in printed URL for basic safety/logging (assumes encoded P@ssw0rd)
print(f"   URL: {url.replace('P%40ssw0rd', '******')}")

try:
    # Parse the URL (expecting the format: mssql://user:password@host:port/dbname)
    parsed = urlparse(url)
    password = unquote(parsed.password)  # Decode percent-encoded password
    print(f"   Decoded password: {password}")
    if password == 'P@ssw0rd':
        print("   SUCCESS: Password decoded correctly.")
    else:
        print(f"   FAILURE: Password decoded incorrectly: {password}")
except Exception as e:
    # Print any error encountered during URL parsing or decoding
    print(f"   FAILURE: Parsing failed: {e}")
