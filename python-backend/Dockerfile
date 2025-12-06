################################################################################
# Dockerfile for Python FastAPI Backend (Clean Architecture Project)
#
# This container setup:
#   - Uses a minimal, secure Python 3.12 base image
#   - Installs all application dependencies from requirements.txt
#   - Copies in application code and entrypoint script
#   - Ensures entrypoint is executable
#   - Prepares the backend to start via a robust entrypoint
#   - Exposes port 8001 for HTTP traffic (matching uvicorn config)
################################################################################

# ---- 1. Use a minimal and secure Python base image ----
FROM python:3.12-slim

# ---- 2. Set the working directory inside the container ----
WORKDIR /app

# ---- 3. Copy dependency file and install dependencies ----
COPY requirements.txt .              
RUN pip install --no-cache-dir -r requirements.txt   # Install all required packages

# ---- 4. Copy all application code into the container ----
COPY . .                             

# ---- 5. Add entrypoint script and ensure it is executable ----
COPY entrypoint.sh .                 
RUN chmod +x entrypoint.sh           

# ---- 6. Expose backend port for HTTP traffic (matching uvicorn/entrypoint config) ----
EXPOSE 8001                          

# ---- 7. Use a robust entrypoint for DB health/migrations/seeding before starting app ----
# Entrypoint waits for DB, runs alembic & seeds admin
ENTRYPOINT ["/bin/bash", "/app/entrypoint.sh"]

# ---- 8. Default command to run backend (can be overridden in docker-compose/CMD) ----
# Launch FastAPI app via uvicorn
CMD ["uvicorn", "main:app", "--host", "0.0.0.0", "--port", "8001"]
