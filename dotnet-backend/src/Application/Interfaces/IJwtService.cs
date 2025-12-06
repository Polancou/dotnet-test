using Domain.Entities;

namespace Application.Interfaces;

/// <summary>
/// Provides methods for generating JSON Web Tokens (JWT), 
/// including access tokens and refresh tokens, for user authentication.
/// </summary>
public interface IJwtService
{
    /// <summary>
    /// Generates a JWT access token for the specified user.
    /// </summary>
    /// <param name="user">The user entity for which to generate the access token.</param>
    /// <returns>
    /// A signed JWT access token string that contains claims identifying the user.
    /// </returns>
    string GenerateAccessToken(User user);

    /// <summary>
    /// Generates a secure random JWT refresh token.
    /// </summary>
    /// <returns>
    /// A cryptographically strong random refresh token string.
    /// </returns>
    string GenerateRefreshToken();
}
