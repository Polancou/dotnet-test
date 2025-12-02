using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;

namespace Application.Services;

public class DocumentService : IDocumentService
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DocumentService(IDocumentRepository documentRepository, IUnitOfWork unitOfWork)
    {
        _documentRepository = documentRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<DocumentResponse> UploadDocumentAsync(DocumentUploadRequest request, int userId)
    {
        // In real app, save stream to storage (AWS S3, Azure Blob, or local disk)
        // For now, we simulate saving to a path
        var storagePath = $"/uploads/{Guid.NewGuid()}_{request.FileName}";
        
        var document = new Document(request.FileName, storagePath, request.ContentType, request.Content.Length, userId);
        
        await _documentRepository.AddAsync(document);
        await _unitOfWork.SaveChangesAsync();

        return new DocumentResponse(document.Id, document.FileName, document.ContentType, document.FileSize, document.IsProcessed, document.AnalysisResult, document.CreationDate);
    }

    public async Task<IEnumerable<DocumentResponse>> GetUserDocumentsAsync(int userId)
    {
        var documents = await _documentRepository.GetByUserIdAsync(userId);
        return documents.Select(d => new DocumentResponse(d.Id, d.FileName, d.ContentType, d.FileSize, d.IsProcessed, d.AnalysisResult, d.CreationDate));
    }
}
