using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services;

public class CsvService(IUserRepository userRepository, IUnitOfWork unitOfWork, IPasswordHasher passwordHasher) : ICsvService
{
    public async Task<CsvProcessResult> ProcessUserBulkUploadAsync(Stream csvStream)
    {
        var successCount = 0;
        var failureCount = 0;
        var errors = new List<string>();

        using var reader = new StreamReader(csvStream);
        
        // Skip header
        var header = await reader.ReadLineAsync();
        if (header == null) return new CsvProcessResult(0, 0, new List<string> { "Empty file" });

        int lineNumber = 1;
        while (!reader.EndOfStream)
        {
            lineNumber++;
            var line = await reader.ReadLineAsync();
            if (string.IsNullOrWhiteSpace(line)) continue;

            var values = line.Split(',');
            if (values.Length < 4)
            {
                failureCount++;
                errors.Add($"Line {lineNumber}: Invalid format. Expected Username,Email,Password,Role");
                continue;
            }

            var username = values[0].Trim();
            var email = values[1].Trim();
            var password = values[2].Trim();
            var roleString = values[3].Trim();

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                failureCount++;
                errors.Add($"Line {lineNumber}: Missing required fields.");
                continue;
            }

            if (!Enum.TryParse<UserRole>(roleString, true, out var role))
            {
                failureCount++;
                errors.Add($"Line {lineNumber}: Invalid role '{roleString}'.");
                continue;
            }

            // Check if user exists
            var existingUser = await userRepository.GetByUsernameAsync(username);
            if (existingUser != null)
            {
                failureCount++;
                errors.Add($"Line {lineNumber}: User '{username}' already exists.");
                continue;
            }

            try
            {
                var passwordHash = passwordHasher.Hash(password);
                var user = new User(username, email, passwordHash, role);
                await userRepository.AddAsync(user);
                successCount++;
            }
            catch (Exception ex)
            {
                failureCount++;
                errors.Add($"Line {lineNumber}: Error creating user. {ex.Message}");
            }
        }

        if (successCount > 0)
        {
            await unitOfWork.SaveChangesAsync();
        }

        return new CsvProcessResult(successCount, failureCount, errors);
    }
}
