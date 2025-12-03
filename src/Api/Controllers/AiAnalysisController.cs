using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AiAnalysisController(IAiAnalysisService aiAnalysisService) : ControllerBase
{
    [HttpPost("analyze")]
    public async Task<IActionResult> Analyze(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded.");

        using var stream = file.OpenReadStream();
        var result = await aiAnalysisService.AnalyzeDocumentAsync(stream, file.FileName);
        
        return Ok(new { Result = result });
    }
}
