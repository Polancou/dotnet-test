using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services;

/// <summary>
/// Service for handling operations related to documents, such as uploads, retrieval, and analysis.
/// </summary>
/// <remarks>
/// Handles bulk CSV uploads (e.g., for user import), tracks document processing, 
/// and logs relevant events.
/// </remarks>
public class DocumentService(
    IDocumentRepository documentRepository,
    IUnitOfWork unitOfWork,
    IFileStorageService fileStorageService,
    ICsvService csvService,
    IEventLogService eventLogService)
    : IDocumentService
{
    /// <summary>
    /// Handles the upload of a new document, optionally processes it (e.g., user bulk upload via CSV),
    /// persists the document metadata, and logs the upload event.
    /// </summary>
    /// <param name="request">The document upload request containing the file stream and metadata.</param>
    /// <param name="userId">ID of the uploading user.</param>
    /// <param name="role">Role of the uploading user (for permission checks).</param>
    /// <returns>
    /// A <see cref="DocumentResponse"/> object representing the stored document and any validation errors that occurred.
    /// </returns>
    /// <exception cref="UnauthorizedAccessException">
    /// Thrown if a user without ADMIN role attempts a bulk user upload.
    /// </exception>
    public async Task<DocumentResponse> UploadDocumentAsync(DocumentUploadRequest request, int userId, UserRole role)
    {
        // Ensure stream is at the beginning and is seekable
        if (!request.Content.CanSeek)
        {
            throw new InvalidOperationException("The provided stream must be seekable to determine file size.");
        }

        request.Content.Position = 0;
        var fileSize = request.Content.Length;
        request.Content.Position = 0; // Reset position

        // Read the entire stream content into memory to avoid stream disposal issues
        // This allows us to use the content multiple times (for CSV processing and file storage)
        byte[] fileContent;
        using (var memoryStream = new MemoryStream())
        {
            await request.Content.CopyToAsync(memoryStream);
            fileContent = memoryStream.ToArray();
        }

        List<string>? validationErrors = null; // Collects validation or processing errors, if any.
        string? processingResult = null;

        // Process document FIRST if it's a CSV bulk user upload (before saving to storage)
        if (request.ProcessType == "UserBulk" && request.ContentType == "text/csv")
        {
            // Only admins are allowed to perform bulk user CSV uploads.
            if (role != UserRole.Admin)
            {
                throw new UnauthorizedAccessException("Only admins can perform bulk user uploads.");
            }

            // Process the CSV using a new stream from the file content
            using var csvStream = new MemoryStream(fileContent);
            var result = await csvService.ProcessUserBulkUploadAsync(csvStream);
            processingResult = $"Processed: {result.SuccessCount} success, {result.FailureCount} failed.";
            validationErrors = result.Errors;
        }

        // Save the uploaded file to persistent storage using the file content
        using var storageStream = new MemoryStream(fileContent);
        var storagePath = await fileStorageService.SaveFileAsync(storageStream, request.FileName);

        // Create the document entity to be stored in the database.
        var document = new Document(request.FileName, storagePath, request.ContentType, fileSize, userId);

        // Mark document as processed if processing was performed
        if (processingResult != null)
        {
            document.MarkAsProcessed(processingResult);
        }

        // Persist the document metadata to the repository.
        await documentRepository.AddAsync(document);
        await unitOfWork.SaveChangesAsync();

        // Log the document upload event for auditing purposes.
        await eventLogService.LogEventAsync("Document Upload", $"User uploaded {request.FileName}", userId);

        // Return a response DTO with document details and any processing validations.
        return new DocumentResponse(
            document.Id,
            document.FileName,
            document.ContentType,
            document.FileSize,
            document.IsProcessed,
            document.AnalysisResult,
            document.CreationDate,
            validationErrors
        );
    }

    /// <summary>
    /// Retrieves all documents that belong to a specific user.
    /// </summary>
    /// <param name="userId">ID of the user whose documents are fetched.</param>
    /// <returns>
    /// IEnumerable of <see cref="DocumentResponse"/> representing the user's documents.
    /// </returns>
    public async Task<IEnumerable<DocumentResponse>> GetUserDocumentsAsync(int userId)
    {
        // Query repository for documents belonging to the user.
        var documents = await documentRepository.GetByUserIdAsync(userId);

        // Log the document list event
        await eventLogService.LogEventAsync("Document List", "User retrieved their document list", userId);

        // Project each document entity into a response DTO. No validation errors returned here (null).
        return documents.Select(d =>
            new DocumentResponse(
                d.Id,
                d.FileName,
                d.ContentType,
                d.FileSize,
                d.IsProcessed,
                d.AnalysisResult,
                d.CreationDate,
                null
            )
        );
    }

    /// <summary>
    /// Updates the analysis result for a specific document, marks it as processed, and persists the change.
    /// </summary>
    /// <param name="documentId">ID of the document to update.</param>
    /// <param name="analysisResult">The result string or output to associate with the document.</param>
    public async Task UpdateAnalysisResultAsync(int documentId, string analysisResult)
    {
        // Fetch document from repository by ID.
        var document = await documentRepository.GetByIdAsync(documentId);
        if (document != null)
        {
            // Mark document as processed with the given analysis result.
            document.MarkAsProcessed(analysisResult);
            // Commit changes to the data store.
            await unitOfWork.SaveChangesAsync();
        }
        // If document is not found, silently do nothing (could alternatively log/fail, not specified here).
    }

    /// <summary>
    /// Asynchronously downloads a document after verifying the user's ownership.
    /// </summary>
    public async Task<(Stream Content, string ContentType, string FileName)> DownloadDocumentAsync(int documentId,
        int userId)
    {
        var document = await documentRepository.GetByIdAsync(documentId);
        if (document == null)
        {
            throw new KeyNotFoundException($"Document with ID {documentId} not found.");
        }

        if (document.UploadedByUserId != userId)
        {
            throw new UnauthorizedAccessException("You are not authorized to download this document.");
        }

        // Log the download event
        await eventLogService.LogEventAsync("Document Download",
            $"User downloaded document ID {documentId} ({document.FileName})", userId);

        var (stream, contentType) = await fileStorageService.GetFileAsync(document.StoragePath);
        return (stream, contentType, document.FileName);
    }

    /// <summary>
    /// Asynchronously deletes a document and its stored file after verifying the user's ownership.
    /// </summary>
    public async Task DeleteDocumentAsync(int documentId, int userId)
    {
        var document = await documentRepository.GetByIdAsync(documentId);
        if (document == null)
        {
            throw new KeyNotFoundException($"Document with ID {documentId} not found.");
        }

        // Allow deletion if user owns it. 
        if (document.UploadedByUserId != userId)
        {
            throw new UnauthorizedAccessException("You are not authorized to delete this document.");
        }

        // Delete file from storage (S3) first
        await fileStorageService.DeleteFileAsync(document.StoragePath);

        // Delete metadata from repository
        documentRepository.Remove(document);
        await unitOfWork.SaveChangesAsync();

        // Log the delete event
        await eventLogService.LogEventAsync("Document Delete",
            $"User deleted document ID {documentId} ({document.FileName})", userId);
    }
}
