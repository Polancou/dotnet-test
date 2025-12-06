using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Services;

/// <summary>
/// Service responsible for generating JWT access tokens and refresh tokens for authentication.
/// </summary>
public class JwtService(IConfiguration configuration) : IJwtService
{
    /// <summary>
    /// Generates a JWT access token for the given user.
    /// </summary>
    /// <param name="user">The user entity for which the token is generated.</param>
    /// <returns>A JWT access token as a string, valid for 15 minutes.</returns>
    public string GenerateAccessToken(User user)
    {
        // Build the symmetric security key from configuration
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!));

        // Create signing credentials using HMAC SHA256
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // Define claims for the JWT token (Subject, Email, Username, Role)
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),                  // Unique identifier (User ID) as subject
            new Claim(JwtRegisteredClaimNames.Email, user.Email),                        // Email of the user
            new Claim(ClaimTypes.Name, user.Username),                                   // Username
            new Claim(ClaimTypes.Role, user.Role.ToString())                             // Role (as string)
        };

        // Build the JWT token with issuer, audience, claims and expiry time (15 minutes)
        var token = new JwtSecurityToken(
            issuer: configuration["Jwt:Issuer"],
            audience: configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddMinutes(15),
            signingCredentials: creds
        );

        // Serialize the JWT token to a string and return
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// Generates a refresh token using a GUID. The token is Base64-encoded for easy storage.
    /// </summary>
    /// <returns>A refresh token as a Base64 string.</returns>
    public string GenerateRefreshToken()
    {
        // Use a new GUID as the refresh token (simple and unique), and encode as Base64 string for storage/transmission
        return Convert.ToBase64String(Guid.NewGuid().ToByteArray());
    }
}
