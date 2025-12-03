namespace Application.DTOs;

public record DocumentUploadRequest(string FileName, Stream Content, string ContentType, string? ProcessType = null);
public record DocumentResponse(int Id, string FileName, string ContentType, long FileSize, bool IsProcessed, string? AnalysisResult, DateTime CreationDate, List<string>? ValidationErrors = null);
