"""
Comprehensive feature test script for the backend service.

This script simulates an integrated flow:
  1. Registers a new user and logs them in to retrieve an access JWT.
  2. Uploads a document (dummy file) as the authenticated user.
  3. Triggers an AI analysis (mock, if Gemini API unavailable) with a second file.
  4. Retrieves event logs for inspection.
  5. Cleans up local test files.

Each step contains inline comments explaining the purpose and logic for clarity.
"""

import requests
import random
import string
import os

# Base URL for the FastAPI backend service being tested
BASE_URL = "http://127.0.0.1:8001"

def get_random_string(length):
    """
    Generate a random lowercase ASCII string of the given length.

    Args:
        length (int): The number of characters to generate.

    Returns:
        str: A random string of lowercase letters.
    """
    letters = string.ascii_lowercase
    return ''.join(random.choice(letters) for i in range(length))

def test_features_flow():
    """
    Simulates the end-to-end feature flow:
      1. Register and login a new user.
      2. Upload a dummy document as the user.
      3. Trigger AI analysis (mock or real) with a test file.
      4. Retrieve event logs.
      5. Clean up any artifacts.
    """
    # ---- 1. Register and Login ----
    username = f"user_{get_random_string(5)}"      # Unique username per run
    email = f"{username}@example.com"
    password = "StrongPassword123!"

    print(f"Registering user {username}...")
    # Attempt to register the user; API may allow duplicate test users
    requests.post(
        f"{BASE_URL}/auth/register",
        json={"username": username, "email": email, "password": password}
    )

    print("Logging in...")
    # Log in to receive JWT access token
    login_res = requests.post(
        f"{BASE_URL}/auth/login",
        json={"username": username, "password": password}
    )
    if login_res.status_code != 200:
        print(f"Login failed: {login_res.text}")
        return  # Abort if login fails

    token = login_res.json()["access_token"]
    headers = {"Authorization": f"Bearer {token}"}  # Use bearer JWT for subsequent requests

    # ---- 2. Upload Document ----
    print("Uploading document...")
    # Create a dummy file to upload
    with open("test_doc.txt", "w") as f:
        f.write("This is a test document content.")

    # Open and upload the file under "/documents/upload" endpoint
    with open("test_doc.txt", "rb") as f:
        files = {"file": ("test_doc.txt", f, "text/plain")}
        upload_res = requests.post(
            f"{BASE_URL}/documents/upload",
            headers=headers,
            files=files
        )

    if upload_res.status_code == 200:
        print("Document uploaded successfully")
        print(upload_res.json())  # Show result (probably file path or metadata)
    else:
        print(f"Upload failed: {upload_res.text}")

    # ---- 3. AI Analysis ----
    print("Analyzing document (Mock)...")
    # Create a test invoice file to trigger mock invoice analysis logic
    with open("test_invoice.txt", "w") as f:
        f.write("invoice content")  # Should be detected as 'Invoice' by mock

    with open("test_invoice.txt", "rb") as f:
        files = {"file": ("test_invoice.txt", f, "text/plain")}
        analyze_res = requests.post(
            f"{BASE_URL}/aianalysis/analyze",
            headers=headers,
            files=files
        )

    if analyze_res.status_code == 200:
        print("Analysis successful")
        print(analyze_res.json())  # Should be mock invoice data if Gemini is disabled
    else:
        print(f"Analysis failed: {analyze_res.text}")

    # ---- 4. Get Logs ----
    print("Getting event logs...")
    # Fetch event logs to verify AI and upload events are recorded
    logs_res = requests.get(
        f"{BASE_URL}/eventlogs/",
        headers=headers
    )

    if logs_res.status_code == 200:
        logs = logs_res.json()
        print(f"Logs retrieved: {len(logs)}")
        print(logs)  # Display logs for inspection
    else:
        print(f"Get logs failed: {logs_res.text}")

    # ---- 5. Cleanup ----
    # Remove local test files created
    if os.path.exists("test_doc.txt"):
        os.remove("test_doc.txt")
    if os.path.exists("test_invoice.txt"):
        os.remove("test_invoice.txt")

if __name__ == "__main__":
    # Entry point: run the full features test flow
    test_features_flow()
