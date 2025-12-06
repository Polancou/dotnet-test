using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

/// <summary>
/// Controller for handling user authentication, registration, and token refresh operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService authService) : ControllerBase
{
    /// <summary>
    /// Authenticates the user and returns a JWT token if successful.
    /// </summary>
    /// <param name="request">The login credentials (username/email and password).</param>
    /// <returns>
    /// 200 OK with JWT token and user details on successful authentication;<br/>
    /// 401 Unauthorized if credentials are invalid.
    /// </returns>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            // Call the service to perform login and generate tokens
            var response = await authService.LoginAsync(request);
            return Ok(response);
        }
        catch (UnauthorizedAccessException)
        {
            // Return 401 if authentication fails
            return Unauthorized();
        }
    }

    /// <summary>
    /// Registers a new user with the provided registration information.
    /// </summary>
    /// <param name="request">User registration details.</param>
    /// <returns>
    /// 200 OK if registration is successful;<br/>
    /// 400 Bad Request if registration fails due to invalid data or duplicate accounts.
    /// </returns>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            // Register the user via the service layer
            await authService.RegisterAsync(request);
            return Ok();
        }
        catch (InvalidOperationException ex)
        {
            // Return 400 Bad Request with error message if registration fails
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Refreshes the JWT token using a valid refresh token.
    /// </summary>
    /// <param name="request">The refresh token payload.</param>
    /// <returns>
    /// 200 OK with a new JWT token if refresh token is valid;<br/>
    /// 401 Unauthorized if refresh token is invalid or expired.
    /// </returns>
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
    {
        try
        {
            // Request a new JWT using the refresh token
            var response = await authService.RefreshTokenAsync(request.RefreshToken);
            return Ok(response);
        }
        catch (UnauthorizedAccessException)
        {
            // Return 401 if refresh token is invalid or not found
            return Unauthorized();
        }
    }
}

/// <summary>
/// Represents a request to refresh a JWT using a refresh token.
/// </summary>
public record RefreshTokenRequest(string RefreshToken);
