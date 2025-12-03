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
}

public interface IEventLogService
{
    Task<IEnumerable<Domain.Entities.EventLog>> GetLogsAsync(int userId, Domain.Enums.UserRole role);
}
