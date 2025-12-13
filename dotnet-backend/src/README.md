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

## 🔥 API Capabilities

The backend exposes a comprehensive RESTful API. Below are the key functional areas:

### 🔐 Authentication (`/api/Auth`)
-   **Login**: Authenticate with email/password to receive a JWT and Refresh Token.
-   **Register**: Create a new user account.
-   **Refresh Token**: Obtain a new JWT using a valid refresh token.

### 📄 Document Management (`/api/Documents`)
-   **Upload**: Upload files (PDF, images, etc.) to the system.
-   **My Documents**: Retrieve a list of documents uploaded by the current user.

### 🤖 AI Analysis (`/api/AiAnalysis`)
-   **Analyze Document**: Upload a document to be processed by Google Gemini AI, extracting key insights and summaries.

### 📜 Event Logs (`/api/EventLogs`)
-   **Get Logs**: Retrieve activity logs (audit trails) for the authenticated user.

### 👥 User Management (`/api/Users`) *[Admin Only]*
-   **Get All Users**: List all registered users.
-   **Update Role**: Change a user's role (e.g., from User to Admin).
-   **Delete User**: Permanently remove a user account.

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
    
    # AWS S3 Configuration (Optional - for cloud file storage)
    dotnet user-secrets set "Aws:AccessKeyId" "YOUR_AWS_ACCESS_KEY_ID" --project Api/Api.csproj
    dotnet user-secrets set "Aws:SecretAccessKey" "YOUR_AWS_SECRET_ACCESS_KEY" --project Api/Api.csproj
    dotnet user-secrets set "Aws:Region" "us-east-1" --project Api/Api.csproj
    dotnet user-secrets set "Aws:S3BucketName" "your-bucket-name" --project Api/Api.csproj
    dotnet user-secrets set "Aws:UseS3Storage" "true" --project Api/Api.csproj
    ```
    
    **Note:** If `Aws:UseS3Storage` is set to `false` or not configured, the application will use local filesystem storage instead of S3.

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
