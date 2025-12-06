namespace Application.DTOs;

/// <summary>
/// Represents the request data required for a user to log in.
/// </summary>
/// <param name="Username">
/// The unique username of the user attempting to log in. 
/// This is normally used to identify and authenticate the user.
/// </param>
/// <param name="Password">
/// The plain-text password provided by the user for authentication. 
/// Should be securely transmitted (e.g., over HTTPS).
/// </param>
public record LoginRequest(string Username, string Password);

/// <summary>
/// Represents the response returned after a successful login operation,
/// including tokens and session information.
/// </summary>
/// <param name="AccessToken">
/// The JWT or similar token to be used for authorizing API requests.
/// </param>
/// <param name="RefreshToken">
/// The token to obtain a new access token after expiration, used to maintain user sessions.
/// </param>
/// <param name="ExpiresInMinutes">
/// The number of minutes until the access token expires.
/// </param>
/// <param name="Role">
/// The role or authorization level assigned to the authenticated user
/// (e.g., "User", "Admin").
/// </param>
public record LoginResponse(string AccessToken, string RefreshToken, int ExpiresInMinutes, string Role);

/// <summary>
/// Represents the request data required for registering a new user.
/// </summary>
/// <param name="Username">
/// The desired username for the new user. Must be unique in the system.
/// </param>
/// <param name="Email">
/// The email address of the user. Used for contact and account recovery.
/// </param>
/// <param name="Password">
/// The plain-text password for the user account. Should be stored securely (hashed).
/// </param>
public record RegisterRequest(string Username, string Email, string Password);
