"""
main.py

FastAPI application entry point.

- Configures the FastAPI app and essential middleware.
- Sets up CORS (Cross-Origin Resource Sharing) for frontend/backend communication.
- Includes routers for authentication, user, document, AI analysis, and event log endpoints.
- Root endpoint available for health check or friendly API greeting.

Author: (your name/team)
"""

from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware

# Import all route/controller modules to register API routers
from src.infrastructure.persistence.database import create_tables
from src.api.controllers import (
    auth_controller,
    users_controller,
    documents_controller,
    ai_analysis_controller,
    event_logs_controller
)

# FastAPI application instance
app = FastAPI()

# ---------------------------
# Configure CORS middleware
# ---------------------------
# This allows the frontend (e.g., React, Angular) to communicate with the API.
# NOTE: Allowing all origins is insecure for production; specify trusted client URLs instead.
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],  # In production, specify the exact client URL (e.g., ["https://frontend.example.com"])
    allow_credentials=True,
    allow_methods=["*"],  # Allow all HTTP methods (GET, POST, PUT, DELETE, etc.)
    allow_headers=["*"],  # Allow all HTTP headers
)

# --------------------------------------------
# Database Table Creation (managed externally)
# --------------------------------------------
# Tables should now be created and managed via Alembic migrations.
# Uncomment below to use automatic table creation (NOT recommended in production):
# try:
#     create_tables()
# except Exception as e:
#     print(f"Error creating tables: {e}")

# ----------------------------------
# Include API route controllers
# ----------------------------------
# Each router contains the endpoint logic for its resource.
app.include_router(auth_controller.router, prefix="/api")           # Authentication endpoints (login, register, etc.)
app.include_router(users_controller.router, prefix="/api")          # User management (CRUD)
app.include_router(documents_controller.router, prefix="/api")      # Document upload, retrieval, etc.
app.include_router(ai_analysis_controller.router, prefix="/api")    # AI-powered analysis endpoints
app.include_router(event_logs_controller.router, prefix="/api")     # Application event/error logging

# ---------------------------
# Root API endpoint
# ---------------------------
@app.get("/")
def read_root():
    """
    Health check or demo root endpoint.

    Returns a friendly greeting message from the Python backend.
    """
    return {"message": "Hello World from Python Backend"}
