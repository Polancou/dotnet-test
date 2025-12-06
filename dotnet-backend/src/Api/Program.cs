// -----------------------------------------------------------------------------------------------------
// Program.cs - Entry point and application startup configuration for the Clean Architecture API.
// -----------------------------------------------------------------------------------------------------

using Application.Interfaces;
using Application.Services;
using Application.Validators;
using FluentValidation;
using FluentValidation.AspNetCore;
using Infrastructure.Persistence;
using Infrastructure.Repositories;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Scalar.AspNetCore;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Configures service dependencies and middleware pipeline for the Clean Architecture API.

// -------------------------------
// Service Configuration
// -------------------------------

// CORS: Allow all origins, methods, and headers to enable cross-origin requests for development.
// In production, consider locking this down for security.
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        corsBuilder =>
        {
            corsBuilder.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
        });
});

// Add controllers and configure JSON options, such as serializing enums as strings.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Enables enum values to be serialized/deserialized as strings in JSON.
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

// Add OpenAPI/Swagger generation for automatic API documentation.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    // Add a Swagger document for this API.
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Clean Architecture API", Version = "v1" });

    // Add JWT bearer authentication support in the Swagger UI.
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] { }
        }
    });
});

// -------------------------------
// Database Context
// -------------------------------

// Register ApplicationDbContext using SQL Server and the configured connection string.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// -------------------------------
// Dependency Injection for Services and Repos
// -------------------------------

// Domain/application-level dependencies.
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IDocumentRepository, DocumentRepository>();
builder.Services.AddScoped<IEventLogRepository, EventLogRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IDocumentService, DocumentService>();
builder.Services.AddScoped<IEventLogService, EventLogService>();
builder.Services.AddScoped<IJwtService, JwtService>();

// Register file storage service: use S3 if configured, otherwise use local filesystem
var useS3Storage = builder.Configuration.GetValue<bool>("Aws:UseS3Storage", false);
if (useS3Storage)
{
    builder.Services.AddScoped<IFileStorageService>(serviceProvider =>
    {
        try
        {
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
            var logger = serviceProvider.GetService<ILogger<Infrastructure.Services.S3FileStorageService>>();
            return new Infrastructure.Services.S3FileStorageService(configuration, logger);
        }
        catch (Exception ex)
        {
            // If S3 configuration is invalid, fall back to local storage
            var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
            logger.LogWarning(ex, "Failed to initialize S3 storage, falling back to local storage");
            return new FileStorageService();
        }
    });
}
else
{
    builder.Services.AddScoped<IFileStorageService, FileStorageService>();
}

builder.Services.AddScoped<ICsvService, CsvService>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IEventNotifier, Api.Services.SignalREventNotifier>();
builder.Services.AddScoped<IAiAnalysisService, AiAnalysisService>();
builder.Services.AddScoped<IUserService, UserService>();

// Real-time communication using SignalR for event log notifications.
builder.Services.AddSignalR();

// Register a default HttpClient for making outbound HTTP requests.
builder.Services.AddHttpClient();

// -------------------------------
// Validators (FluentValidation)
// -------------------------------

// Automatically register validators and enable automatic validation for incoming DTO models.
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>();

// -------------------------------
// JWT Authentication Configuration
// -------------------------------

// Add JWT authentication and specify token validation parameters.
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

var app = builder.Build();

// -------------------------------
// HTTP Request Pipeline Configuration
// -------------------------------

// Configures middlewares and features in the application's request pipeline, including
// exception handling, CORS, HTTPS redirection, authentication, authorization,
// controller endpoints, and SignalR Hubs.

// Enable Swagger UI and Scalar API reference endpoint in development.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    // Scalar UI for API reference browsing.
    app.MapScalarApiReference(options => { options.WithOpenApiRoutePattern("/swagger/v1/swagger.json"); });
}

// Redirect all HTTP requests to HTTPS for security.
app.UseHttpsRedirection();

// Add custom exception handling middleware for global error management.
app.UseMiddleware<Api.Middleware.ExceptionMiddleware>();

// Enable CORS policy ("AllowAll" - see above).
app.UseCors("AllowAll");

// Enable authentication and authorization.
app.UseAuthentication();
app.UseAuthorization();

// Map controllers and endpoints.
app.MapControllers();

// Map real-time SignalR EventLog hub endpoint for event notifications.
app.MapHub<Api.Hubs.EventLogHub>("/hubs/eventLogs");

// -------------------------------
// Database Migration & Seeding
// -------------------------------

// Apply database migrations on startup to ensure the database schema is up to date.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await db.Database.MigrateAsync(); // Apply all pending migrations
}

// Seeds a default Admin user if one does not already exist. This is useful for initial deployments
// and development environments. Logs success or errors to the console.
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var userRepository = services.GetRequiredService<IUserRepository>();
        var passwordHasher = services.GetRequiredService<IPasswordHasher>();
        var unitOfWork = services.GetRequiredService<IUnitOfWork>();

        // Check if the admin user already exists.
        var adminUser = await userRepository.GetByUsernameAsync("admin");
        if (adminUser == null)
        {
            // If not, create the admin user with default credentials and role.
            var passwordHash = passwordHasher.Hash("Admin123!");
            var newAdmin =
                new Domain.Entities.User("admin", "admin@example.com", passwordHash, Domain.Enums.UserRole.Admin);
            await userRepository.AddAsync(newAdmin);
            await unitOfWork.SaveChangesAsync();
            Console.WriteLine("Admin user seeded successfully.");
        }
    }
    catch (Exception ex)
    {
        // Log and report any errors encountered during seeding.
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

// -------------------------------
// Application Start
// -------------------------------

// Starts the application.
app.Run();

// Partial Program class for testability (used for integration testing with ASP.NET).
public partial class Program
{
}
