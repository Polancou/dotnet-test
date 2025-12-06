# .NET Backend (ASP.NET Core)

The original backend implementation using **.NET 9** and **Clean Architecture**.

## 🚀 Technologies

-   **Framework**: .NET 9 (ASP.NET Core)
-   **Database**: SQL Server / Azure SQL Edge (via Entity Framework Core)
-   **Authentication**: JWT (Bearer Token)
-   **Validation**: FluentValidation
-   **Testing**: xUnit, Moq

## 📂 Project Structure

-   `Api`: The REST API entry point and controllers.
-   `Application`: Business logic, DTOs, and Interfaces.
-   `Domain`: Core entities and business rules.
-   `Infrastructure`: Data access, external services implementation.

## 🛠️ Setup & Run

### Prerequisites
-   .NET 9 SDK
-   Docker (for SQL Server)

### Database Setup

1.  Start the SQL Server container:
    ```bash
    docker-compose up -d
    ```
    (Ensure you run this from the root directory where `docker-compose.yml` is located).

2.  Configure User Secrets (for connection string & JWT key):
    ```bash
    dotnet user-secrets init --project Api/Api.csproj
    dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost,1433;Database=CleanArchDb;User Id=sa;Password=YourStrongPassword123!;TrustServerCertificate=True;" --project Api/Api.csproj
    dotnet user-secrets set "Jwt:Key" "ThisIsASecretKeyForJwtTokenGeneration123!" --project Api/Api.csproj
    dotnet user-secrets set "Gemini:ApiKey" "YOUR_API_KEY" --project Api/Api.csproj
    ```

3.  Apply Migrations:
    ```bash
    dotnet ef database update -p ../Infrastructure -s Api
    ```

### Running the Application

Navigate to the `Api` directory and run:
```bash
dotnet run
```
The API will be available at `https://localhost:7001` (or configured port).
Swagger UI: `https://localhost:7001/swagger`.

## 🧪 Testing

Run all unit and integration tests:
```bash
dotnet test
```
