using Application.DTOs;

namespace Application.Interfaces;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request);
    Task RegisterAsync(RegisterRequest request);
}

public interface IDocumentService
{
    Task<DocumentResponse> UploadDocumentAsync(DocumentUploadRequest request, int userId);
    Task<IEnumerable<DocumentResponse>> GetUserDocumentsAsync(int userId);
}
