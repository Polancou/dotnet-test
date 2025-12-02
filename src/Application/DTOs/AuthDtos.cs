namespace Application.DTOs;

public record LoginRequest(string Username, string Password);
public record LoginResponse(string AccessToken, string RefreshToken, int ExpiresInMinutes);
public record RegisterRequest(string Username, string Email, string Password);
