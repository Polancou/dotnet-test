using Domain.Common;
using Domain.Enums;

namespace Domain.Entities;

/// <summary>
/// Represents a user entity in the domain.
/// </summary>
public class User : BaseEntity
{
    /// <summary>
    /// Gets the username of the user.
    /// </summary>
    public string Username { get; private set; }

    /// <summary>
    /// Gets the email address of the user.
    /// </summary>
    public string Email { get; private set; }

    /// <summary>
    /// Gets the hashed password of the user.
    /// </summary>
    public string PasswordHash { get; private set; }

    /// <summary>
    /// Gets the role assigned to the user.
    /// </summary>
    public UserRole Role { get; private set; }

    /// <summary>
    /// Private constructor for EF Core.
    /// </summary>
    private User() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="User"/> class.
    /// </summary>
    /// <param name="username">The username of the user.</param>
    /// <param name="email">The email address of the user.</param>
    /// <param name="passwordHash">The hashed password of the user.</param>
    /// <param name="role">The role assigned to the user.</param>
    /// <exception cref="ArgumentException">Thrown if username, email, or password hash is null or whitespace.</exception>
    public User(string username, string email, string passwordHash, UserRole role) : base(true)
    {
        if (string.IsNullOrWhiteSpace(username)) throw new ArgumentException("Username cannot be empty.", nameof(username));
        if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email cannot be empty.", nameof(email));
        if (string.IsNullOrWhiteSpace(passwordHash)) throw new ArgumentException("Password hash cannot be empty.", nameof(passwordHash));

        Username = username;
        Email = email;
        PasswordHash = passwordHash;
        Role = role;
    }

    /// <summary>
    /// Updates the user's password.
    /// </summary>
    /// <param name="newPasswordHash">The new hashed password.</param>
    /// <exception cref="ArgumentException">Thrown if the new password hash is null or whitespace.</exception>
    public void UpdatePassword(string newPasswordHash)
    {
        if (string.IsNullOrWhiteSpace(newPasswordHash)) throw new ArgumentException("New password hash cannot be empty.", nameof(newPasswordHash));
        PasswordHash = newPasswordHash;
        UpdateModificationDate();
    }
    /// <summary>
    /// Gets the refresh token for the user.
    /// </summary>
    public string? RefreshToken { get; private set; }

    /// <summary>
    /// Gets the expiry time of the refresh token.
    /// </summary>
    public DateTime? RefreshTokenExpiryTime { get; private set; }

    /// <summary>
    /// Updates the user's refresh token.
    /// </summary>
    /// <param name="refreshToken">The new refresh token.</param>
    /// <param name="expiryTime">The expiry time of the new refresh token.</param>
    public void UpdateRefreshToken(string refreshToken, DateTime expiryTime)
    {
        RefreshToken = refreshToken;
        RefreshTokenExpiryTime = expiryTime;
        UpdateModificationDate();
    }
}
