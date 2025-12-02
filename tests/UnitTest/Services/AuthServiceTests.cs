using Application.DTOs;
using Application.Interfaces;
using Application.Services;
using Domain.Entities;
using Domain.Enums;
using Moq;

namespace UnitTest.Services;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IJwtService> _jwtServiceMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _jwtServiceMock = new Mock<IJwtService>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _authService = new AuthService(_userRepositoryMock.Object, _jwtServiceMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnToken_WhenCredentialsAreValid()
    {
        // Arrange
        var request = new LoginRequest("testuser", "password123");
        var user = new User("testuser", "test@example.com", "password123", UserRole.User);
        _userRepositoryMock.Setup(x => x.GetByUsernameAsync(request.Username)).ReturnsAsync(user);
        _jwtServiceMock.Setup(x => x.GenerateAccessToken(user)).Returns("access_token");
        _jwtServiceMock.Setup(x => x.GenerateRefreshToken()).Returns("refresh_token");

        // Act
        var result = await _authService.LoginAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("access_token", result.AccessToken);
        Assert.Equal("refresh_token", result.RefreshToken);
    }

    [Fact]
    public async Task RegisterAsync_ShouldRegisterUser_WhenUserDoesNotExist()
    {
        // Arrange
        var request = new RegisterRequest("testuser", "test@example.com", "password");
        _userRepositoryMock.Setup(x => x.GetByUsernameAsync(request.Username))
            .ReturnsAsync((User?)null);

        // Act
        await _authService.RegisterAsync(request);

        // Assert
        _userRepositoryMock.Verify(x => x.AddAsync(It.Is<User>(u => 
            u.Username == request.Username && 
            u.Email == request.Email && 
            u.Role == UserRole.User)), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_ShouldThrowUnauthorizedAccessException_WhenCredentialsAreInvalid()
    {
        // Arrange
        var request = new LoginRequest("testuser", "wrongpassword");
        var user = new User("testuser", "test@example.com", "password", UserRole.User);
        _userRepositoryMock.Setup(x => x.GetByUsernameAsync(request.Username)).ReturnsAsync(user);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _authService.LoginAsync(request));
    }
}
