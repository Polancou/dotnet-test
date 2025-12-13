# Python Backend (FastAPI)

This is the Python implementation of the backend, designed to mirror the functionality of the .NET backend using **FastAPI** and **Clean Architecture** principles.

## 🚀 Technologies

-   **Framework**: FastAPI
-   **Language**: Python 3.12+
-   **Database**: SQLite (via SQLAlchemy)
-   **Authentication**: JWT (PyJWT), BCrypt
-   **Validation**: Pydantic
-   **AI Integration**: Google Gemini (via REST)

## 📂 Project Structure

```
python-backend/
├── src/
│   ├── domain/       # Entities & Enums
│   ├── application/  # Services & Interfaces
│   ├── infrastructure/ # Repositories & External Services
│   └── api/          # Controllers & Dependencies
├── tests/            # Automated Tests
└── main.py           # Entry Point
```

## 🛠️ Setup & Run

### Prerequisites
-   Python 3.12+
-   pip
-   Docker (optional)

### 1. Environment Configuration

1.  Navigate to the `python-backend` directory.
2.  Copy the example environment file:
    ```bash
    cp .env.example .env
    ```
3.  Open `.env` and configure your variables (e.g., `GEMINI_API_KEY`).

### 2. Local Execution

1.  **Create and activate a virtual environment:**
    ```bash
    python3 -m venv venv
    source venv/bin/activate  # On Windows: venv\Scripts\activate
    ```

2.  **Install dependencies:**
    ```bash
    pip install -r requirements.txt
    ```

3.  **Initialize the database:**
    ```bash
    python create_db.py
    python seed.py
    ```

4.  **Start the server:**
    ```bash
    uvicorn main:app --reload --port 8001
    ```
    - API: `http://127.0.0.1:8001`
    - Docs: `http://127.0.0.1:8001/docs`

### 3. 🐳 Docker Execution

1.  **Build the image:**
    ```bash
    docker build -t python-backend .
    ```

2.  **Run the container:**
    ```bash
    docker run -d -p 8001:8001 --env-file .env --name python-backend python-backend
    ```

### 4. Database Management (Alembic)

To manage schema changes:

**Generate migration:**
```bash
alembic revision --autogenerate -m "Description"
```

**Apply migrations:**
```bash
alembic upgrade head
```

## 🧪 Testing

Run the verification scripts:
Run the unit tests using `pytest`:

```bash
# Activate virtual environment first
source venv/bin/activate

# Run all unit tests
python -m pytest tests/test_unit_cases.py

# Run verification scripts
python tests/test_api.py
python tests/test_features.py
```
