namespace Application.DTOs;

/// <summary>
/// Represents the request data required to upload a document.
/// </summary>
/// <param name="FileName">
/// The name of the document file being uploaded (including extension).
/// </param>
/// <param name="Content">
/// The content stream of the file. This should contain the raw file bytes.
/// </param>
/// <param name="ContentType">
/// The MIME type of the uploaded file (e.g., "application/pdf").
/// </param>
/// <param name="ProcessType">
/// (Optional) The process type or document type hint (e.g., "Invoice" or "Information").
/// </param>
public record DocumentUploadRequest(
    string FileName,
    Stream Content,
    string ContentType,
    string? ProcessType = null
);

/// <summary>
/// Represents the response for a document, including its metadata and analysis state.
/// </summary>
/// <param name="Id">
/// The unique identifier assigned to the document in the system.
/// </param>
/// <param name="FileName">
/// The document's filename (including extension).
/// </param>
/// <param name="ContentType">
/// The MIME type of the document.
/// </param>
/// <param name="FileSize">
/// The size of the file in bytes.
/// </param>
/// <param name="IsProcessed">
/// Whether the document has undergone processing/analysis.
/// </param>
/// <param name="AnalysisResult">
/// Serialized JSON string of the analysis result if processed; otherwise <c>null</c>.
/// </param>
/// <param name="CreationDate">
/// The UTC date and time when the document was uploaded/created.
/// </param>
/// <param name="ValidationErrors">
/// (Optional) List of validation errors, if the document failed validation during upload or processing.
/// Will be <c>null</c> or empty if there are no errors.
/// </param>
public record DocumentResponse(
    int Id,
    string FileName,
    string ContentType,
    long FileSize,
    bool IsProcessed,
    string? AnalysisResult,
    DateTime CreationDate,
    List<string>? ValidationErrors = null
);
