using Application.DTOs;
using Application.Interfaces;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AiAnalysisController(
    IAiAnalysisService aiAnalysisService, 
    IDocumentService documentService) : ControllerBase
{
    [HttpPost("analyze")]
    [Authorize]
    public async Task<IActionResult> Analyze(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded.");

        // 1. Get User ID
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            return Unauthorized();
        }
        
        // 2. Persist Document (Reuse DocumentService)
        // We create a request object manually
        using var stream = file.OpenReadStream();
        var uploadRequest = new DocumentUploadRequest(
            file.FileName,
            stream,
            file.ContentType,
            "AiAnalysis"
        );

        // We use UploadDocumentAsync to save the file and create the entity.
        var roleClaim = User.FindFirst(ClaimTypes.Role);
        var role = roleClaim != null && Enum.TryParse<UserRole>(roleClaim.Value, out var r) ? r : UserRole.User;

        var documentResponse = await documentService.UploadDocumentAsync(uploadRequest, userId, role);

        // 3. Perform Analysis
        if (stream.CanSeek)
        {
            stream.Position = 0;
        }
        
        var analysisResult = await aiAnalysisService.AnalyzeDocumentAsync(stream, file.FileName);

        // 4. Update Document with Result
        var jsonResult = JsonSerializer.Serialize(analysisResult);
        await documentService.UpdateAnalysisResultAsync(documentResponse.Id, jsonResult);
        
        return Ok(new 
        { 
            DocumentId = documentResponse.Id,
            Analysis = analysisResult 
        });
    }
}
