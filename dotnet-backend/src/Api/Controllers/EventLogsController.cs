using Application.Interfaces;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Api.Controllers;

/// <summary>
/// Controller for retrieving event logs associated with the current user.
/// All endpoints require authorization.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EventLogsController(IEventLogService eventLogService) : ControllerBase
{
    /// <summary>
    /// Retrieves the event logs for the current user based on their user ID and role.
    /// </summary>
    /// <returns>
    /// 200 OK with the list of event logs on success; <br/>
    /// 401 Unauthorized if role is invalid or cannot be parsed.
    /// </returns>
    [HttpGet]
    public async Task<IActionResult> GetLogs()
    {
        // Extract the user ID from claims (should always be present due to [Authorize])
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        // Retrieve the user's role from claims as a string
        var roleString = User.FindFirst(ClaimTypes.Role)?.Value;
        
        // Attempt to parse the role string into the UserRole enum
        if (!Enum.TryParse<UserRole>(roleString, out var role))
        {
            // If parsing fails, return 401 Unauthorized with a descriptive message
            return Unauthorized("Invalid role");
        }

        // Retrieve event logs for this user/role combination using the injected service
        var logs = await eventLogService.GetLogsAsync(userId, role);

        // Return an HTTP 200 OK with the logs in the response body
        return Ok(logs);
    }
}
