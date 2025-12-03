using Application.DTOs;

namespace Application.Interfaces;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request);
    Task RegisterAsync(RegisterRequest request);
    Task<LoginResponse> RefreshTokenAsync(string token);
}

public interface IDocumentService
{
    Task<DocumentResponse> UploadDocumentAsync(DocumentUploadRequest request, int userId, Domain.Enums.UserRole role);
    Task<IEnumerable<DocumentResponse>> GetUserDocumentsAsync(int userId);
    Task UpdateAnalysisResultAsync(int documentId, string analysisResult);
}

public interface IEventLogService
{
    Task<IEnumerable<Domain.Entities.EventLog>> GetLogsAsync(int userId, Domain.Enums.UserRole role);
    Task LogEventAsync(string eventType, string details, int? userId = null);
}

public interface IEventNotifier
{
    Task NotifyAsync(string eventType, object data);
}

public interface IAiAnalysisService
{
    Task<AnalysisResultDto> AnalyzeDocumentAsync(Stream fileStream, string fileName);
}

public interface IUserService
{
    Task<IEnumerable<UserDto>> GetAllUsersAsync();
    Task UpdateUserRoleAsync(int userId, Domain.Enums.UserRole newRole);
    Task DeleteUserAsync(int userId);
}
