using Xunit;
using Moq;
using FluentValidation.TestHelper;
using Application.Services;
using Application.Validators;
using Application.DTOs;
using Domain.Entities;
using Application.Interfaces;
using Domain.Enums;
using System.Threading.Tasks;
using System.IO;
using System;
using System.Collections.Generic;

namespace UnitTest.Services
{
    public class AdditionalServiceTests
    {
        private readonly Mock<IUserRepository> _mockUserRepo;
        private readonly Mock<IPasswordHasher> _mockPasswordHasher;
        private readonly Mock<IJwtService> _mockJwtService;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IEventLogService> _mockEventLogService;
        private readonly AuthService _authService;

        public AdditionalServiceTests()
        {
            _mockUserRepo = new Mock<IUserRepository>();
            _mockPasswordHasher = new Mock<IPasswordHasher>();
            _mockJwtService = new Mock<IJwtService>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockEventLogService = new Mock<IEventLogService>();

            _authService = new AuthService(
                _mockUserRepo.Object,
                _mockJwtService.Object,
                _mockUnitOfWork.Object,
                _mockPasswordHasher.Object,
                _mockEventLogService.Object
            );
        }

        // --- 1. AuthService: Login Success ---
        [Fact]
        public async Task Login_ValidCredentials_ReturnsToken()
        {
            // Arrange
            var request = new LoginRequest("testuser", "password123");
            var user = new User("testuser", "test@example.com", "hashed_password", UserRole.User);
            // Reflect: ID might be needed if service uses it, but User ctor doesn't set ID (it's base entity). 
            // We can assume ID is 0 or mocked if strictly needed, but logic uses Username mainly.

            _mockUserRepo.Setup(r => r.GetByUsernameAsync(request.Username)).ReturnsAsync(user);
            _mockPasswordHasher.Setup(h => h.Verify("password123", "hashed_password")).Returns(true);
            _mockJwtService.Setup(j => j.GenerateAccessToken(user)).Returns("valid_access_token");
            _mockJwtService.Setup(j => j.GenerateRefreshToken()).Returns("valid_refresh_token");

            // Act
            var result = await _authService.LoginAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("valid_access_token", result.AccessToken);
            Assert.Equal("valid_refresh_token", result.RefreshToken);
        }

        // --- 2. AuthService: Login Invalid Username Tests ---
        [Fact]
        public async Task Login_InvalidUsername_ThrowsException()
        {
            // Arrange
            var request = new LoginRequest("nonexistent", "password123");
            _mockUserRepo.Setup(r => r.GetByUsernameAsync(request.Username)).ReturnsAsync((User?)null);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _authService.LoginAsync(request));
        }

        // --- 3. AuthService: Login Invalid Password ---
        [Fact]
        public async Task Login_InvalidPassword_ThrowsUnauthorized()
        {
            // Arrange
            var request = new LoginRequest("testuser", "wrongpassword");
            var user = new User("testuser", "test@example.com", "hashed_password", UserRole.User);

            _mockUserRepo.Setup(r => r.GetByUsernameAsync(request.Username)).ReturnsAsync(user);
            _mockPasswordHasher.Setup(h => h.Verify("wrongpassword", "hashed_password")).Returns(false);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _authService.LoginAsync(request));
        }


        // --- 4. Validator: LoginRequestValidator Valid ---
        [Fact]
        public void LoginRequestValidator_ValidDto_Passes()
        {
            // Arrange
            var validator = new LoginRequestValidator();
            var model = new LoginRequest("user", "password");

            // Act
            var result = validator.TestValidate(model);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        // --- 5. Validator: LoginRequestValidator Empty Username ---
        [Fact]
        public void LoginRequestValidator_EmptyUsername_Fails()
        {
            // Arrange
            var validator = new LoginRequestValidator();
            var model = new LoginRequest("", "password");

            // Act
            var result = validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Username);
        }

        // --- 6. Validator: RegisterRequestValidator Weak Password ---
        [Fact]
        public void RegisterRequestValidator_ShortPassword_Fails()
        {
            // Arrange
            var validator = new RegisterRequestValidator();
            var model = new RegisterRequest("user", "email@test.com", "123");

            // Act
            var result = validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Password);
        }

        // --- 7. DocumentService: Upload Processing ---
        [Fact]
        public async Task UploadDocument_ValidFile_CallsRepository()
        {
            // Arrange
            var mockDocRepo = new Mock<IDocumentRepository>();
            var mockFileStorage = new Mock<IFileStorageService>();
            var mockUnitOfWork2 = new Mock<IUnitOfWork>();
            var mockCsvService = new Mock<ICsvService>();
            var mockEventLog = new Mock<IEventLogService>();

            var docService = new DocumentService(
                mockDocRepo.Object,
                mockUnitOfWork2.Object,
                mockFileStorage.Object,
                mockCsvService.Object,
                mockEventLog.Object
            );

            var content = new MemoryStream(new byte[] { 1, 2, 3 });
            var request = new DocumentUploadRequest("test.pdf", content, "application/pdf");
            var userId = 1;
            var role = UserRole.User;

            mockFileStorage.Setup(f => f.SaveFileAsync(It.IsAny<Stream>(), It.IsAny<string>()))
                .ReturnsAsync("path/to/test.pdf");

            // Act
            await docService.UploadDocumentAsync(request, userId, role);

            // Assert
            mockDocRepo.Verify(
                r => r.AddAsync(It.Is<Document>(d => d.FileName == "test.pdf" && d.UploadedByUserId == userId)),
                Times.Once);
            mockUnitOfWork2.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        // --- 8. DocumentService: GetDocument Verify Access (Mocked Repository Return) ---
        // Since GetDocumentByIdAsync is not in the interface/service shown (only GetUserDocumentsAsync and Download),
        // I will test GetUserDocumentsAsync instead.
        [Fact]
        public async Task GetUserDocuments_ReturnsMappedDtos()
        {
            // Arrange
            var mockDocRepo = new Mock<IDocumentRepository>();
            var docService = new DocumentService(
                mockDocRepo.Object,
                new Mock<IUnitOfWork>().Object,
                new Mock<IFileStorageService>().Object,
                new Mock<ICsvService>().Object,
                new Mock<IEventLogService>().Object
            );

            var doc = new Document("test.txt", "path", "text/plain", 100, 1);
            // We need to set Id, but setter is private/protected. 
            // In unit tests with EF Core or pure mocks, usually we assume mapping works.
            // Or we simulate repository returning it. 

            mockDocRepo.Setup(r => r.GetByUserIdAsync(1)).ReturnsAsync(new List<Document> { doc });

            // Act
            var result = await docService.GetUserDocumentsAsync(1);

            var list = result.ToList();
            Assert.Single(list);
            Assert.Equal("test.txt", list[0].FileName);
        }

        // --- 9. DocumentService: Delete Document Wrong User ---
        [Fact]
        public async Task DeleteDocument_UserDoesNotOwn_ThrowsUnauthorized()
        {
            // Arrange
            var mockDocRepo = new Mock<IDocumentRepository>();
            var docService = new DocumentService(
                mockDocRepo.Object,
                new Mock<IUnitOfWork>().Object,
                new Mock<IFileStorageService>().Object,
                new Mock<ICsvService>().Object,
                new Mock<IEventLogService>().Object
            );

            var doc = new Document("test.txt", "path", "text/plain", 100, 2); // Owned by user 2
            mockDocRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(doc);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                docService.DeleteDocumentAsync(1, 1)); // User 1 requests delete
        }

        // --- 10. Validator: RegisterRequestValidator Invalid Email ---
        [Fact]
        public void RegisterRequestValidator_InvalidEmail_Fails()
        {
            // Arrange
            var validator = new RegisterRequestValidator();
            var model = new RegisterRequest("user", "not-an-email", "StrongPassword!");

            // Act
            var result = validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Email);
        }
    }
}
