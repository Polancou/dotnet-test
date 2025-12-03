using Application.Interfaces;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EventLogsController(IEventLogService eventLogService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetLogs()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var roleString = User.FindFirst(ClaimTypes.Role)?.Value;
        
        if (!Enum.TryParse<UserRole>(roleString, out var role))
        {
            return Unauthorized("Invalid role");
        }

        var logs = await eventLogService.GetLogsAsync(userId, role);
        return Ok(logs);
    }
}
