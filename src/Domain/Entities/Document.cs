using Domain.Common;

namespace Domain.Entities;

public class Document : BaseEntity
{
    public string FileName { get; private set; }
    public string StoragePath { get; private set; }
    public string ContentType { get; private set; }
    public long FileSize { get; private set; }
    public bool IsProcessed { get; private set; }
    public string? AnalysisResult { get; private set; } // JSON or text summary
    public int UploadedByUserId { get; private set; }
    public User UploadedByUser { get; private set; }

    private Document() { } // EF Core

    public Document(string fileName, string storagePath, string contentType, long fileSize, int uploadedByUserId) : base(true)
    {
        if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentException("File name cannot be empty.", nameof(fileName));
        if (string.IsNullOrWhiteSpace(storagePath)) throw new ArgumentException("Storage path cannot be empty.", nameof(storagePath));
        
        FileName = fileName;
        StoragePath = storagePath;
        ContentType = contentType;
        FileSize = fileSize;
        UploadedByUserId = uploadedByUserId;
        IsProcessed = false;
    }

    public void MarkAsProcessed(string result)
    {
        IsProcessed = true;
        AnalysisResult = result;
        UpdateModificationDate();
    }
}
