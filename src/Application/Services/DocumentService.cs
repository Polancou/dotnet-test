using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services;

public class DocumentService(
    IDocumentRepository documentRepository,
    IUnitOfWork unitOfWork,
    IFileStorageService fileStorageService,
    ICsvService csvService,
    IEventLogService eventLogService)
    : IDocumentService
{
    public async Task<DocumentResponse> UploadDocumentAsync(DocumentUploadRequest request, int userId, UserRole role)
    {
        // Save file to storage
        var storagePath = await fileStorageService.SaveFileAsync(request.Content, request.FileName);
        
        var document = new Document(request.FileName, storagePath, request.ContentType, request.Content.Length, userId);
        
        List<string>? validationErrors = null;

        // Process if needed
        if (request.ProcessType == "UserBulk" && request.ContentType == "text/csv")
        {
            if (role != UserRole.Admin)
            {
                throw new UnauthorizedAccessException("Only admins can perform bulk user uploads.");
            }
            
            if (request.Content.CanSeek)
            {
                request.Content.Position = 0;
                var result = await csvService.ProcessUserBulkUploadAsync(request.Content);
                var resultMessage = $"Processed: {result.SuccessCount} success, {result.FailureCount} failed.";
                document.MarkAsProcessed(resultMessage);
                validationErrors = result.Errors;
            }
            else
            {
                 document.MarkAsProcessed("Error: Stream not seekable for processing.");
            }
        }
        
        await documentRepository.AddAsync(document);
        await unitOfWork.SaveChangesAsync();

        await eventLogService.LogEventAsync("Document Upload", $"User uploaded {request.FileName}", userId);

        return new DocumentResponse(document.Id, document.FileName, document.ContentType, document.FileSize, document.IsProcessed, document.AnalysisResult, document.CreationDate, validationErrors);
    }

    public async Task<IEnumerable<DocumentResponse>> GetUserDocumentsAsync(int userId)
    {
        var documents = await documentRepository.GetByUserIdAsync(userId);
        return documents.Select(d => new DocumentResponse(d.Id, d.FileName, d.ContentType, d.FileSize, d.IsProcessed, d.AnalysisResult, d.CreationDate, null));
    }

    public async Task UpdateAnalysisResultAsync(int documentId, string analysisResult)
    {
        var document = await documentRepository.GetByIdAsync(documentId);
        if (document != null)
        {
            document.MarkAsProcessed(analysisResult);
            await unitOfWork.SaveChangesAsync();
        }
    }
}
