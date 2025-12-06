################################################################################
# Dockerfile for .NET Clean Architecture API (src/Api)
# Multi-stage build: build, publish, and runtime optimization.
# Comments throughout explain each step for clarity and maintainability.
################################################################################

# --------------------------------------------------------------------------------
# Build stage: Restore dependencies and build the application using .NET SDK image
# --------------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

# Set the working directory for subsequent commands
WORKDIR /src

# Copy csproj files for each layer to leverage Docker build cache on restore
COPY ["src/Api/Api.csproj", "src/Api/"]
COPY ["src/Application/Application.csproj", "src/Application/"]
COPY ["src/Domain/Domain.csproj", "src/Domain/"]
COPY ["src/Infrastructure/Infrastructure.csproj", "src/Infrastructure/"]

# Restore NuGet dependencies for the API project and all referenced projects
RUN dotnet restore "src/Api/Api.csproj"

# Copy all remaining source code into the build container
COPY . .

# Set the working directory to the API project root for build
WORKDIR "/src/src/Api"

# Build the API project in Release mode, outputting results to dedicated directory
RUN dotnet build "Api.csproj" -c Release -o /app/build

# --------------------------------------------------------------------------------
# Publish stage: Publish the app with all required runtime assets
# --------------------------------------------------------------------------------
FROM build AS publish

# Publish the .NET API (self-contained, no app host) for minimal docker image size
RUN dotnet publish "Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# --------------------------------------------------------------------------------
# Final runtime stage: Use lightweight .NET runtime image for optimized containers
# --------------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final

# Set working directory for the runtime container
WORKDIR /app

# Install netcat (nc) to allow scripting of wait-for-it in entrypoint
RUN apt-get update \
    && apt-get install -y netcat-openbsd \
    && rm -rf /var/lib/apt/lists/*

# Copy published application from the previous stage into /app
COPY --from=publish /app/publish .

# Copy custom entrypoint script and set executable permission
COPY src/Api/entrypoint.sh .
RUN chmod +x entrypoint.sh

# Expose API HTTP port
EXPOSE 8080

# Set entrypoint to custom shell script (allows "wait-for-it" on dependencies/services)
ENTRYPOINT ["./entrypoint.sh"]

# Default command: run the API .NET application DLL
CMD ["dotnet", "Api.dll"]

################################################################################
# End of Dockerfile: Ready for efficient, secure containerized deployment.
################################################################################
