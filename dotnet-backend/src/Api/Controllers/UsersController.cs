using Application.DTOs;
using Application.Interfaces;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

/// <summary>
/// Controller for managing user accounts (admin only). Allows listing all users, updating user roles, and deleting users.
/// </summary>
[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/[controller]")]
public class UsersController(IUserService userService) : ControllerBase
{
    /// <summary>
    /// Retrieves all user accounts.
    /// </summary>
    /// <returns>
    /// 200 OK with a list of all users.
    /// </returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetAllUsers()
    {
        // Retrieve list of all users from the service
        var users = await userService.GetAllUsersAsync();
        return Ok(users);
    }

    /// <summary>
    /// Updates the role of a specific user.
    /// </summary>
    /// <param name="id">The user ID to update.</param>
    /// <param name="request">Request body specifying the new role.</param>
    /// <returns>
    /// 204 No Content if update is successful;<br/>
    /// 404 Not Found if user does not exist.
    /// </returns>
    [HttpPut("{id}/role")]
    public async Task<IActionResult> UpdateUserRole(int id, [FromBody] UpdateUserRoleRequest request)
    {
        try
        {
            // Update the user's role using the service layer
            await userService.UpdateUserRoleAsync(id, request.NewRole);
            return NoContent(); // Role updated successfully
        }
        catch (KeyNotFoundException)
        {
            // User not found, return 404 Not Found
            return NotFound();
        }
    }

    /// <summary>
    /// Permanently deletes a user account by ID.
    /// </summary>
    /// <param name="id">The ID of the user to delete.</param>
    /// <returns>
    /// 204 No Content if deletion is successful;<br/>
    /// 404 Not Found if user does not exist.
    /// </returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        try
        {
            // Delete the user via the service
            await userService.DeleteUserAsync(id);
            return NoContent(); // User deleted successfully
        }
        catch (KeyNotFoundException)
        {
            // User not found, return 404 Not Found
            return NotFound();
        }
    }
}

/// <summary>
/// Request payload for updating a user's role.
/// </summary>
public class UpdateUserRoleRequest
{
    /// <summary>
    /// The new role to assign to the user.
    /// </summary>
    public UserRole NewRole { get; set; }
}
