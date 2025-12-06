# .NET Backend (ASP.NET Core)

The original backend implementation using **.NET 9** and **Clean Architecture**.

## 🚀 Technologies

-   **Framework**: .NET 9 (ASP.NET Core)
-   **Database**: SQL Server / Azure SQL Edge (via Entity Framework Core)
-   **Authentication**: JWT (Bearer Token)
-   **Validation**: FluentValidation
-   **Testing**: xUnit, Moq

## 📂 Project Structure

-   `src/Api`: The REST API entry point and controllers.
-   `src/Application`: Business logic, DTOs, and Interfaces.
-   `src/Domain`: Core entities and business rules.
-   `src/Infrastructure`: Data access, external services implementation.

## 🛠️ Setup & Run

### Prerequisites
-   .NET 9 SDK
-   Docker (for SQL Server)

### 1. Database Setup

1.  **Start SQL Server:**
    Navigate to the project root (where `docker-compose.yml` is) and run:
    ```bash
    docker-compose up -d
    ```

2.  **Apply Migrations:**
    From the `dotnet-backend` directory:
    ```bash
    dotnet ef database update -p src/Infrastructure -s src/Api
    ```

### 2. Environment Configuration (User Secrets)

It is recommended to use User Secrets for local development to avoid committing sensitive data.

```bash
# Initialize secrets
dotnet user-secrets init --project src/Api/Api.csproj

# Set Connection String
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost,1433;Database=CleanArchDb;User Id=sa;Password=YourStrongPassword123!;TrustServerCertificate=True;" --project src/Api/Api.csproj

# Set JWT Key
dotnet user-secrets set "Jwt:Key" "ThisIsASecretKeyForJwtTokenGeneration123!" --project src/Api/Api.csproj

# Set Gemini API Key (Optional)
dotnet user-secrets set "Gemini:ApiKey" "YOUR_API_KEY" --project src/Api/Api.csproj
```

### 3. Local Execution

Run the API from the `dotnet-backend` directory:

```bash
dotnet run --project src/Api/Api.csproj
```
- API: `http://localhost:5000` (or configured port)
- Swagger: `http://localhost:5000/swagger`

### 4. 🐳 Docker Execution

To build and run the backend as a container:

1.  **Build the image:**
    ```bash
    # Run from dotnet-backend directory
    docker build -t dotnet-backend -f src/Api/Dockerfile .
    ```

2.  **Run the container:**
    ```bash
    docker run -d -p 8080:8080 --name dotnet-backend dotnet-backend
    ```

## 🧪 Testing

Run all unit and integration tests:
```bash
dotnet test
```
