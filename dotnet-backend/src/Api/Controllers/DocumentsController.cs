using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Api.Controllers;

/// <summary>
/// Controller responsible for managing document upload and retrieval.
/// All endpoints require authorization.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DocumentsController(IDocumentService documentService) : ControllerBase
{
    /// <summary>
    /// Uploads a document for the current user.
    /// </summary>
    /// <param name="file">The file to be uploaded (from multipart/form-data).</param>
    /// <param name="type">Optional document type as a query string.</param>
    /// <returns>
    /// 200 OK with document details if upload is successful;<br/>
    /// 400 Bad Request if no file is uploaded.
    /// </returns>
    [HttpPost("upload")]
    public async Task<IActionResult> Upload(IFormFile file, [FromQuery] string? type)
    {
        // Validate that a file has been uploaded
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded.");

        // Retrieve the current user's ID from claims; fallback to 0 if not present (should not happen with [Authorize])
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        // Retrieve the current user's role from claims
        var roleString = User.FindFirst(ClaimTypes.Role)?.Value;
        // Attempt to parse the user's role; if fails, 'role' is set to default (UserRole = 0)
        Enum.TryParse<Domain.Enums.UserRole>(roleString, out var role);

        // Use a memory stream to read the uploaded file.
        // Note: In production, consider streaming directly or using temp files for large uploads
        using var stream = new MemoryStream();
        await file.CopyToAsync(stream);
        var content = stream.ToArray(); // Copy file content into a byte array

        // Construct a document upload request object with the file info
        var request = new DocumentUploadRequest(file.FileName, new MemoryStream(content), file.ContentType, type);

        // Upload the document using the service, associating with the user and their role
        var response = await documentService.UploadDocumentAsync(request, userId, role);

        // Return the created document's details as JSON
        return Ok(response);
    }

    /// <summary>
    /// Retrieves all documents for the current user.
    /// </summary>
    /// <returns>
    /// 200 OK with a list of the user's document summaries.
    /// </returns>
    [HttpGet]
    public async Task<IActionResult> GetMyDocuments()
    {
        // Retrieve the user ID from claims
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        // Retrieve the documents for the current user
        var documents = await documentService.GetUserDocumentsAsync(userId);
        // Return the documents as JSON
        return Ok(documents);
    }

    /// <summary>
    /// Downloads a specific document.
    /// </summary>
    /// <param name="id">The ID of the document to download.</param>
    /// <returns>
    /// 200 OK with the file stream if successful;<br/>
    /// 404 Not Found if document doesn't exist;<br/>
    /// 403 Forbidden if user doesn't own the document.
    /// </returns>
    [HttpGet("{id}/download")]
    public async Task<IActionResult> DownloadDocument(int id)
    {
        try
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var (stream, contentType, fileName) = await documentService.DownloadDocumentAsync(id, userId);

            return File(stream, contentType, fileName);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    /// <summary>
    /// Deletes a specific document.
    /// </summary>
    /// <param name="id">The ID of the document to delete.</param>
    /// <returns>
    /// 204 No Content if successful;<br/>
    /// 404 Not Found if document doesn't exist;<br/>
    /// 403 Forbidden if user doesn't own the document.
    /// </returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteDocument(int id)
    {
        try
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            await documentService.DeleteDocumentAsync(id, userId);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }
}
