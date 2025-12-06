# Clean Architecture System (.NET & Python)

This repository contains a complete full-stack application with a **Vue 3** frontend and two alternative backend implementations: one in **.NET 9** and another in **Python (FastAPI)**. Both backends follow the **Clean Architecture** pattern and provide identical functionality.

## 📦 System Components

| Component | Path | Description |
|-----------|------|-------------|
| **Frontend** | [`client/`](client/README.md) | Vue 3 + Vite + TypeScript application. |
| **.NET Backend** | [`dotnet-backend/`](dotnet-backend/src/README.md) | ASP.NET Core 9 Clean Architecture implementation. |
| **Python Backend** | [`python-backend/`](python-backend/README.md) | FastAPI Clean Architecture implementation. |

## 🚀 Key Features

-   **Multi-Backend Support**: Seamlessly switch between .NET and Python backends.
-   **Clean Architecture**: Separation of concerns (Domain, Application, Infrastructure, API).
-   **AI Integration**: Document analysis using Google Gemini.
-   **Authentication**: Secure JWT-based auth with Role-Based Access Control (RBAC).
-   **User Management**: Admin dashboard for managing users and roles.
-   **Bulk Operations**: CSV upload for bulk user creation.

## 🛠️ Quick Start

### 1. Choose a Backend

**Option A: Run .NET Backend**
Run with Docker Compose profile:
```bash
docker-compose --profile dotnet up
```
Or follow manual instructions in [`dotnet-backend/src/README.md`](dotnet-backend/src/README.md).
-   Runs on `https://localhost:7001` (local) or `http://localhost:5000` (docker).

**Option B: Run Python Backend**
Run with Docker Compose profile:
```bash
docker-compose --profile python up
```
Or follow manual instructions in [`python-backend/README.md`](python-backend/README.md).
-   Runs on `http://127.0.0.1:8001` (local) or `http://localhost:8001` (docker).

### 2. Run the Frontend

Follow instructions in [`client/README.md`](client/README.md).
1.  `cd client`
2.  `npm install`
3.  `npm run dev`
4.  Open `http://localhost:5173`.

> **Note**: Ensure the frontend is pointing to the correct backend port in its configuration (default is usually set for .NET, update `.env` or proxy config if using Python).

## 🧪 Testing

-   **.NET**: `dotnet test`
-   **Python**: `python tests/test_features.py`

## 📄 License

This project is licensed under the MIT License.