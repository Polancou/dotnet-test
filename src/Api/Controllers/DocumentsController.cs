using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DocumentsController(IDocumentService documentService) : ControllerBase
{
    [HttpPost("upload")]
    public async Task<IActionResult> Upload(IFormFile file, [FromQuery] string? type)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded.");

        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var roleString = User.FindFirst(ClaimTypes.Role)?.Value;
        Enum.TryParse<Domain.Enums.UserRole>(roleString, out var role);

        using var stream = new MemoryStream();
        await file.CopyToAsync(stream);
        var content = stream.ToArray(); // In real app, pass stream directly if possible or use temp file

        var request = new DocumentUploadRequest(file.FileName, new MemoryStream(content), file.ContentType, type);
        var response = await documentService.UploadDocumentAsync(request, userId, role);

        return Ok(response);
    }

    [HttpGet]
    public async Task<IActionResult> GetMyDocuments()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var documents = await documentService.GetUserDocumentsAsync(userId);
        return Ok(documents);
    }
}
