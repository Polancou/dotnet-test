using Application.DTOs;
using Application.Interfaces;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;

namespace Api.Controllers;

/// <summary>
/// Controller for handling AI-based document analysis operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AiAnalysisController(
    IAiAnalysisService aiAnalysisService, 
    IDocumentService documentService) : ControllerBase
{
    /// <summary>
    /// Accepts a document file, saves it, performs AI analysis, and updates the document with the analysis results.
    /// </summary>
    /// <param name="file">The file to analyze (uploaded via multipart/form-data).</param>
    /// <returns>
    /// 200 OK with document ID and analysis results if successful;<br/>
    /// 400 if no file is provided;<br/>
    /// 401 if the user cannot be identified.
    /// </returns>
    [HttpPost("analyze")]
    [Authorize]
    public async Task<IActionResult> Analyze(IFormFile file)
    {
        // Validate that a file is provided in the request
        if (file == null || file.Length == 0)
        {
            // Return a 400 Bad Request if no file is uploaded
            return BadRequest("No file uploaded.");
        }

        // 1. Retrieve the User ID from claims
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            // Return 401 Unauthorized if user ID claim is missing or invalid
            return Unauthorized();
        }
        
        // 2. Persist Document by uploading it through DocumentService
        // Open a read stream for the uploaded file
        using var stream = file.OpenReadStream();

        // Construct a document upload request, specifying the analysis source
        var uploadRequest = new DocumentUploadRequest(
            file.FileName,
            stream,
            file.ContentType,
            "AiAnalysis"
        );

        // Determine the user's role from claims (defaults to UserRole.User on fail)
        var roleClaim = User.FindFirst(ClaimTypes.Role);
        var role = roleClaim != null && Enum.TryParse<UserRole>(roleClaim.Value, out var r) ? r : UserRole.User;

        // Upload the document and obtain the persisted document entity
        var documentResponse = await documentService.UploadDocumentAsync(uploadRequest, userId, role);

        // 3. Perform AI Analysis on the uploaded file content
        // Ensure the stream is at the beginning before analysis
        if (stream.CanSeek)
        {
            stream.Position = 0;
        }
        
        // Analyze the document stream using the AI analysis service
        var analysisResult = await aiAnalysisService.AnalyzeDocumentAsync(stream, file.FileName);

        // 4. Update the persisted document with the analysis results 
        // Serialize the analysis result to JSON for storage
        var jsonResult = JsonSerializer.Serialize(analysisResult);

        // Update the document in storage/database with the analysis data
        await documentService.UpdateAnalysisResultAsync(documentResponse.Id, jsonResult);
        
        // Return 200 OK with both the document ID and the raw analysis result to the client
        return Ok(new 
        { 
            DocumentId = documentResponse.Id,
            Analysis = analysisResult 
        });
    }
}
