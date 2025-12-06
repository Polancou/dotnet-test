using Domain.Enums;

namespace Application.DTOs;

/// <summary>
/// Data Transfer Object representing a user.
/// Used for exposing user information in API responses.
/// </summary>
public class UserDto
{
    /// <summary>
    /// The unique identifier of the user.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// The user's username, used for login or display.
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// The user's email address.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// The role assigned to the user (e.g., User, Admin).
    /// </summary>
    public UserRole Role { get; set; }

    /// <summary>
    /// Indicates whether the user account is active.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// The UTC date and time when the user account was created.
    /// </summary>
    public DateTime CreationDate { get; set; }
}
