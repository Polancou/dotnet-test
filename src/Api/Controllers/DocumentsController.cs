using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DocumentsController : ControllerBase
{
    private readonly IDocumentService _documentService;

    public DocumentsController(IDocumentService documentService)
    {
        _documentService = documentService;
    }

    [HttpPost("upload")]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded.");

        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        using var stream = new MemoryStream();
        await file.CopyToAsync(stream);
        var content = stream.ToArray(); // In real app, pass stream directly if possible or use temp file

        var request = new DocumentUploadRequest(file.FileName, new MemoryStream(content), file.ContentType);
        var response = await _documentService.UploadDocumentAsync(request, userId);

        return Ok(response);
    }

    [HttpGet]
    public async Task<IActionResult> GetMyDocuments()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var documents = await _documentService.GetUserDocumentsAsync(userId);
        return Ok(documents);
    }
}
