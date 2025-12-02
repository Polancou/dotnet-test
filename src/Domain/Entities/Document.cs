using Domain.Common;

namespace Domain.Entities;

/// <summary>
/// Represents a document entity in the system, storing metadata about uploaded files.
/// </summary>
public class Document : BaseEntity
{
    /// <summary>
    /// Gets the original name of the file.
    /// </summary>
    public string FileName { get; private set; }

    /// <summary>
    /// Gets the path where the file is stored.
    /// </summary>
    public string StoragePath { get; private set; }

    /// <summary>
    /// Gets the content type (MIME type) of the file.
    /// </summary>
    public string ContentType { get; private set; }

    /// <summary>
    /// Gets the size of the file in bytes.
    /// </summary>
    public long FileSize { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the document has been processed.
    /// </summary>
    public bool IsProcessed { get; private set; }

    /// <summary>
    /// Gets the result of the analysis performed on the document, typically in JSON or text summary format.
    /// </summary>
    public string? AnalysisResult { get; private set; } // JSON or text summary

    /// <summary>
    /// Gets the ID of the user who uploaded the document.
    /// </summary>
    public int UploadedByUserId { get; private set; }

    /// <summary>
    /// Gets the user who uploaded the document.
    /// </summary>
    public User UploadedByUser { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Document"/> class.
    /// This private constructor is primarily for use by ORM frameworks like EF Core.
    /// </summary>
    private Document() { } // EF Core

    /// <summary>
    /// Initializes a new instance of the <see cref="Document"/> class with essential details.
    /// </summary>
    /// <param name="fileName">The original name of the file.</param>
    /// <param name="storagePath">The path where the file is stored.</param>
    /// <param name="contentType">The content type (MIME type) of the file.</param>
    /// <param name="fileSize">The size of the file in bytes.</param>
    /// <param name="uploadedByUserId">The ID of the user who uploaded the document.</param>
    /// <exception cref="ArgumentException">Thrown if <paramref name="fileName"/> or <paramref name="storagePath"/> is null or whitespace.</exception>
    public Document(string fileName, string storagePath, string contentType, long fileSize, int uploadedByUserId) : base(true)
    {
        if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentException("File name cannot be empty.", nameof(fileName));
        if (string.IsNullOrWhiteSpace(storagePath)) throw new ArgumentException("Storage path cannot be empty.", nameof(storagePath));
        
        FileName = fileName;
        StoragePath = storagePath;
        ContentType = contentType;
        FileSize = fileSize;
        UploadedByUserId = uploadedByUserId;
        IsProcessed = false; // A newly uploaded document is not processed by default
    }

    /// <summary>
    /// Marks the document as processed and stores the analysis result.
    /// </summary>
    /// <param name="result">The analysis result, typically a JSON string or a text summary.</param>
    public void MarkAsProcessed(string result)
    {
        IsProcessed = true;
        AnalysisResult = result;
        UpdateModificationDate(); // Update the modification date when processing status changes
    }
}
