using Domain.Common;

namespace Domain.Entities;

public class User : BaseEntity
{
    public string Username { get; private set; }
    public string Email { get; private set; }
    public string PasswordHash { get; private set; }
    public int RoleId { get; private set; }
    public Role Role { get; private set; }

    private User() { } // EF Core

    public User(string username, string email, string passwordHash, int roleId) : base(true)
    {
        if (string.IsNullOrWhiteSpace(username)) throw new ArgumentException("Username cannot be empty.", nameof(username));
        if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email cannot be empty.", nameof(email));
        if (string.IsNullOrWhiteSpace(passwordHash)) throw new ArgumentException("Password hash cannot be empty.", nameof(passwordHash));

        Username = username;
        Email = email;
        PasswordHash = passwordHash;
        RoleId = roleId;
    }

    public void UpdatePassword(string newPasswordHash)
    {
        if (string.IsNullOrWhiteSpace(newPasswordHash)) throw new ArgumentException("New password hash cannot be empty.", nameof(newPasswordHash));
        PasswordHash = newPasswordHash;
        UpdateModificationDate();
    }
}
