using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services;

public class DocumentService(
    IDocumentRepository documentRepository,
    IUnitOfWork unitOfWork,
    IFileStorageService fileStorageService,
    ICsvService csvService)
    : IDocumentService
{
    public async Task<DocumentResponse> UploadDocumentAsync(DocumentUploadRequest request, int userId, UserRole role)
    {
        // Save file to storage
        var storagePath = await fileStorageService.SaveFileAsync(request.Content, request.FileName);
        
        var document = new Document(request.FileName, storagePath, request.ContentType, request.Content.Length, userId);
        
        // Process if needed
        if (request.ProcessType == "UserBulk" && request.ContentType == "text/csv")
        {
            if (role != UserRole.Admin)
            {
                throw new UnauthorizedAccessException("Only admins can perform bulk user uploads.");
            }
            // Reset stream position if possible, or we should have copied it. 
            // FileStorageService consumed the stream? 
            // SaveFileAsync implementation: await fileStream.CopyToAsync(fileStreamOutput);
            // CopyToAsync advances the position. We need to reset it or use a copy.
            // Since request.Content is likely a MemoryStream (from Controller), we can try Seek.
            if (request.Content.CanSeek)
            {
                request.Content.Position = 0;
                var result = await csvService.ProcessUserBulkUploadAsync(request.Content);
                var resultMessage = $"Processed: {result.SuccessCount} success, {result.FailureCount} failed. Errors: {string.Join("; ", result.Errors.Take(5))}";
                document.MarkAsProcessed(resultMessage);
            }
            else
            {
                 document.MarkAsProcessed("Error: Stream not seekable for processing.");
            }
        }
        
        await documentRepository.AddAsync(document);
        await unitOfWork.SaveChangesAsync();

        return new DocumentResponse(document.Id, document.FileName, document.ContentType, document.FileSize, document.IsProcessed, document.AnalysisResult, document.CreationDate);
    }

    public async Task<IEnumerable<DocumentResponse>> GetUserDocumentsAsync(int userId)
    {
        var documents = await documentRepository.GetByUserIdAsync(userId);
        return documents.Select(d => new DocumentResponse(d.Id, d.FileName, d.ContentType, d.FileSize, d.IsProcessed, d.AnalysisResult, d.CreationDate));
    }
}
