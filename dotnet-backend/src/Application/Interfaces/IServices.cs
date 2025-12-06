using Application.DTOs;

namespace Application.Interfaces;

/// <summary>
/// Defines authentication services, such as login, registration, and token refresh.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Authenticates a user and generates relevant login response (including tokens).
    /// </summary>
    /// <param name="request">Login request containing credentials.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. 
    /// The task result contains a <see cref="LoginResponse"/> if authentication succeeds; otherwise, an error is thrown.
    /// </returns>
    Task<LoginResponse> LoginAsync(LoginRequest request);

    /// <summary>
    /// Registers a new user with the provided registration details.
    /// </summary>
    /// <param name="request">User registration details.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RegisterAsync(RegisterRequest request);

    /// <summary>
    /// Issues a new set of tokens using a valid refresh token.
    /// </summary>
    /// <param name="token">The refresh token.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. 
    /// The task result contains a <see cref="LoginResponse"/> with new tokens if the refresh token is valid.
    /// </returns>
    Task<LoginResponse> RefreshTokenAsync(string token);
}

/// <summary>
/// Defines services related to document operations such as upload, retrieval, and analysis update.
/// </summary>
public interface IDocumentService
{
    /// <summary>
    /// Uploads a document and returns the resulting document response.
    /// </summary>
    /// <param name="request">The upload request with document data.</param>
    /// <param name="userId">ID of the user uploading the document.</param>
    /// <param name="role">Role of the user (can influence permissions/processing).</param>
    /// <returns>
    /// A task representing the asynchronous operation. 
    /// The task result contains the uploaded document response.
    /// </returns>
    Task<DocumentResponse> UploadDocumentAsync(DocumentUploadRequest request, int userId, Domain.Enums.UserRole role);

    /// <summary>
    /// Retrieves all documents belonging to the specified user.
    /// </summary>
    /// <param name="userId">ID of the user whose documents are to be retrieved.</param>
    /// <returns>
    /// A task representing the asynchronous operation.
    /// The task result contains a collection of document responses.
    /// </returns>
    Task<IEnumerable<DocumentResponse>> GetUserDocumentsAsync(int userId);

    /// <summary>
    /// Updates the analysis result of a given document.
    /// </summary>
    /// <param name="documentId">ID of the document to update.</param>
    /// <param name="analysisResult">The analysis result string to be stored.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task UpdateAnalysisResultAsync(int documentId, string analysisResult);
}

/// <summary>
/// Defines services for logging and retrieving event logs.
/// </summary>
public interface IEventLogService
{
    /// <summary>
    /// Retrieves logs visible to the user or administrator, filtered by user and role.
    /// </summary>
    /// <param name="userId">ID of the user requesting logs (used for filtering).</param>
    /// <param name="role">Role of the user (can filter or expand access).</param>
    /// <returns>
    /// A task representing the asynchronous operation.
    /// The task result contains a collection of event logs.
    /// </returns>
    Task<IEnumerable<Domain.Entities.EventLog>> GetLogsAsync(int userId, Domain.Enums.UserRole role);

    /// <summary>
    /// Writes a new event log entry.
    /// </summary>
    /// <param name="eventType">Type/category of the event (e.g., "login", "document_upload").</param>
    /// <param name="details">Details about the event to log.</param>
    /// <param name="userId">Optional ID of the user associated with the event.</param>
    /// <returns>A task representing the asynchronous log operation.</returns>
    Task LogEventAsync(string eventType, string details, int? userId = null);
}

/// <summary>
/// Defines an event notification service, typically for propagating real-time system events.
/// </summary>
public interface IEventNotifier
{
    /// <summary>
    /// Notifies listeners or external systems about a specific event type, with accompanying data.
    /// </summary>
    /// <param name="eventType">The event type string.</param>
    /// <param name="data">Associated data payload.</param>
    /// <returns>A task representing the asynchronous notification operation.</returns>
    Task NotifyAsync(string eventType, object data);
}

/// <summary>
/// Defines AI-powered document analysis services.
/// </summary>
public interface IAiAnalysisService
{
    /// <summary>
    /// Analyzes the given document via an AI model and returns the analysis (e.g., summary, classification).
    /// </summary>
    /// <param name="fileStream">Stream containing the document data.</param>
    /// <param name="fileName">The original filename (may provide context to the analysis engine).</param>
    /// <returns>
    /// A task representing the asynchronous operation.
    /// The result contains an <see cref="AnalysisResultDto"/> describing the outcome.
    /// </returns>
    Task<AnalysisResultDto> AnalyzeDocumentAsync(Stream fileStream, string fileName);
}

/// <summary>
/// Defines user administration and management services, such as listing, updating roles, and deleting users.
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Retrieves all users in the system (typically for admin use).
    /// </summary>
    /// <returns>
    /// A task representing the asynchronous operation.
    /// The result contains a collection of <see cref="UserDto"/> objects.
    /// </returns>
    Task<IEnumerable<UserDto>> GetAllUsersAsync();

    /// <summary>
    /// Updates the role of a specified user.
    /// </summary>
    /// <param name="userId">ID of the user whose role is to be changed.</param>
    /// <param name="newRole">The new role to assign to the user.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task UpdateUserRoleAsync(int userId, Domain.Enums.UserRole newRole);

    /// <summary>
    /// Permanently deletes a user from the system.
    /// </summary>
    /// <param name="userId">ID of the user to delete.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task DeleteUserAsync(int userId);
}
