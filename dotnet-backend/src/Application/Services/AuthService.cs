using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services;

/// <summary>
/// Service responsible for handling authentication-related operations,
/// such as user login, registration, and token refresh.
/// </summary>
public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtService _jwtService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IEventLogService _eventLogService;

    /// <summary>
    /// Initializes an instance of <see cref="AuthService"/> with its required dependencies.
    /// </summary>
    public AuthService(
        IUserRepository userRepository,
        IJwtService jwtService,
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        IEventLogService eventLogService)
    {
        _userRepository = userRepository;
        _jwtService = jwtService;
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _eventLogService = eventLogService;
    }

    /// <summary>
    /// Authenticates a user by username and password, issues JWT tokens on success, and logs the event.
    /// </summary>
    /// <param name="request">Login request containing username and password.</param>
    /// <returns>A <see cref="LoginResponse"/> containing access and refresh tokens, expiry info, and user role.</returns>
    /// <exception cref="UnauthorizedAccessException">Thrown if user credentials are invalid.</exception>
    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        // Fetch the user by username from the repository.
        var user = await _userRepository.GetByUsernameAsync(request.Username);

        // If user not found or password doesn't match, throw.
        if (user == null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid credentials");
        }

        // Generate JWT access and refresh tokens.
        var accessToken = _jwtService.GenerateAccessToken(user);
        var refreshToken = _jwtService.GenerateRefreshToken();

        // Update user's refresh token and its expiry date.
        user.UpdateRefreshToken(refreshToken, DateTime.UtcNow.AddDays(7));
        await _unitOfWork.SaveChangesAsync();

        // Log the login event.
        await _eventLogService.LogEventAsync("User Interaction", $"User {user.Username} logged in.", user.Id);

        // Return the login response object.
        return new LoginResponse(accessToken, refreshToken, 15, user.Role.ToString());
    }

    /// <summary>
    /// Registers a new user with the provided registration information.
    /// </summary>
    /// <param name="request">Registration details such as username, email, and password.</param>
    /// <returns>An asynchronous operation.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the username already exists.</exception>
    public async Task RegisterAsync(RegisterRequest request)
    {
        // Check for an existing user with the given username.
        var existingUser = await _userRepository.GetByUsernameAsync(request.Username);
        if (existingUser != null)
        {
            throw new InvalidOperationException("Username already exists");
        }

        // Hash the user's password for storage.
        var passwordHash = _passwordHasher.Hash(request.Password);

        // Create a new user entity with the 'User' role by default.
        var user = new User(request.Username, request.Email, passwordHash, UserRole.User); // Default role User

        // Add the user to the repository and persist the changes.
        await _userRepository.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();
    }

    /// <summary>
    /// Renews JWT access and refresh tokens using an existing, valid refresh token.
    /// </summary>
    /// <param name="token">The refresh token to use for refreshing authentication.</param>
    /// <returns>
    /// A <see cref="LoginResponse"/> with fresh access and refresh tokens, expiry info, and user role.
    /// </returns>
    /// <exception cref="UnauthorizedAccessException">Thrown if the refresh token is invalid or expired.</exception>
    public async Task<LoginResponse> RefreshTokenAsync(string token)
    {
        // Retrieve the user associated with the provided refresh token.
        var user = await _userRepository.GetByRefreshTokenAsync(token);

        // Check if user is found and the refresh token is still valid.
        if (user == null || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
        {
            throw new UnauthorizedAccessException("Invalid or expired refresh token");
        }

        // Issue new JWT access and refresh tokens.
        var newAccessToken = _jwtService.GenerateAccessToken(user);
        var newRefreshToken = _jwtService.GenerateRefreshToken();

        // Update user's refresh token and its new expiry time.
        user.UpdateRefreshToken(newRefreshToken, DateTime.UtcNow.AddDays(7));
        await _unitOfWork.SaveChangesAsync();

        // Return the login response with the new tokens.
        return new LoginResponse(newAccessToken, newRefreshToken, 15, user.Role.ToString());
    }
}
