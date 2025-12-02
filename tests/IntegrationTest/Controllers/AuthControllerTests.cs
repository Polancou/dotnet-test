using System.Net.Http.Json;
using Application.DTOs;

namespace IntegrationTest.Controllers;

public class AuthControllerTests(CustomWebApplicationFactory<Program> factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task Register_ShouldReturnOk_WhenRequestIsValid()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new RegisterRequest("newuser", "new@example.com", "password123");

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        response.EnsureSuccessStatusCode(); // Status Code 200-299
    }
}
