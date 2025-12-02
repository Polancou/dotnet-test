# Clean Architecture .NET 9 & Vue 3 Project

This project is a modern web application built with **.NET 9** (Backend) and **Vue 3** (Frontend), following **Clean Architecture** principles.

## 🚀 Technologies

- **Backend**: .NET 9, Entity Framework Core, Azure SQL Edge (Docker), FluentValidation, JWT Authentication.
- **Frontend**: Vue 3, Vite, Pinia, Tailwind CSS 4.
- **Testing**: xUnit, Moq, Microsoft.AspNetCore.Mvc.Testing.
- **Containerization**: Docker.

## 📂 Project Structure

- `src/Domain`: Enterprise logic and entities.
- `src/Application`: Business logic, interfaces, and DTOs.
- `src/Infrastructure`: External concerns (Database, Auth, File System).
- `src/Api`: RESTful API entry point.
- `client`: Vue 3 Frontend application.

## 🛠️ Setup & Run

### Prerequisites
- .NET 9 SDK
- Node.js & npm
- Docker

### Backend
1. Navigate to the root directory.
2. Run `docker-compose up -d` (Ensure you have a `docker-compose.yml` for SQL Server).
3. **User Secrets**:
   ```bash
   dotnet user-secrets init --project src/Api/Api.csproj
   dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost,1433;Database=CleanArchDb;User Id=sa;Password=YourStrongPassword123!;TrustServerCertificate=True;" --project src/Api/Api.csproj
   dotnet user-secrets set "Jwt:Key" "ThisIsASecretKeyForJwtTokenGeneration123!" --project src/Api/Api.csproj
   ```
4. **Database Migration**:
   ```bash
   dotnet ef migrations add InitialCreate -p src/Infrastructure -s src/Api
   dotnet ef database update -p src/Infrastructure -s src/Api
   ```
5. Run `dotnet run --project src/Api/Api.csproj`.

### Frontend
1. Navigate to `client`.
2. Run `npm install`.
3. Run `npm run dev`.

## 🧪 Testing
- Run `dotnet test` to execute all tests.