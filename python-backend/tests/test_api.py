"""
Integration API test script for user registration, login, and protected endpoint access.

This script simulates the primary API flow:
  1. Registers a new user with a random username.
  2. Logs in with that user.
  3. Attempts to access a protected `/users/` endpoint with the obtained JWT.
  
Helpful for quickly verifying the overall authentication and authorization flow
of the backend service.

Instructions:
- Ensure your development server is running at BASE_URL.
- Run this script directly as `python test_api.py`.
"""
import requests
import random
import string

# Base URL where the FastAPI backend is hosted
BASE_URL = "http://127.0.0.1:8001"

def get_random_string(length):
    """
    Generate a random lowercase ASCII string of the given length.
    
    Args:
        length (int): Length of the random string.
    Returns:
        str: Randomly-generated string.
    """
    letters = string.ascii_lowercase
    return ''.join(random.choice(letters) for i in range(length))

def test_api_flow():
    """
    Full end-to-end API test:
      1. Register a new user
      2. Login to get a JWT token
      3. Use the token to access a protected endpoint
    """
    # ---- STEP 1: Register a new user ----
    username = f"user_{get_random_string(5)}"                       # Unique username each run
    email = f"{username}@example.com"                               # Dummy email
    password = "StrongPassword123!"                                 # Static strong password

    register_data = {
        "username": username,
        "email": email,
        "password": password
    }
    print(f"Registering user {username}...")

    # Make POST request to /auth/register
    response = requests.post(f"{BASE_URL}/auth/register", json=register_data)
    if response.status_code == 200:
        print("Registration successful")
    else:
        print(f"Registration failed: {response.text}")
        return  # Stop test on registration failure

    # ---- STEP 2: Login with the new user ----
    login_data = {
        "username": username,
        "password": password
    }
    print("Logging in...")

    # Make POST request to /auth/login
    response = requests.post(f"{BASE_URL}/auth/login", json=login_data)
    if response.status_code == 200:
        print("Login successful")
        token_data = response.json()
        access_token = token_data["access_token"]
        print(f"Access Token (truncated): {access_token[:20]}...")
    else:
        print(f"Login failed: {response.text}")
        return  # Stop test on login failure

    # ---- STEP 3: Access a protected route (/users/) using JWT ----
    # Compose Authorization header as required by FastAPI security utilities
    headers = {"Authorization": f"Bearer {access_token}"}
    print("Getting all users (protected endpoint)...")

    # Make GET request to /users/ with JWT in Authorization header
    response = requests.get(f"{BASE_URL}/users/", headers=headers)
    if response.status_code == 200:
        users = response.json()
        print(f"Found {len(users)} users")
        print(users)
    else:
        print(f"Get users failed: {response.text}")

if __name__ == "__main__":
    # Run the API flow test when script is executed directly
    test_api_flow()
