using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services;

/// <summary>
/// Provides service methods for handling CSV-related operations including bulk user upload.
/// </summary>
public class CsvService(
    IUserRepository userRepository, 
    IUnitOfWork unitOfWork, 
    IPasswordHasher passwordHasher
) : ICsvService
{
    /// <summary>
    /// Processes a CSV stream for bulk user uploads.
    /// Parses the CSV data line by line, validates input, creates new users where possible, 
    /// and tracks success/failure counts and error messages.
    /// </summary>
    /// <param name="csvStream">A stream representing the uploaded CSV file containing user details.</param>
    /// <returns>
    /// A CsvProcessResult object containing the number of successful and failed user creations, 
    /// as well as any error messages encountered while processing.
    /// </returns>
    public async Task<CsvProcessResult> ProcessUserBulkUploadAsync(Stream csvStream)
    {
        var successCount = 0;
        var failureCount = 0;
        var errors = new List<string>();

        // Initialize StreamReader to read the incoming CSV stream.
        using var reader = new StreamReader(csvStream);
        
        // Read the header line to skip it, as it is assumed to contain column names.
        var header = await reader.ReadLineAsync();
        if (header == null)
        {
            // If file is empty (i.e. no header), return immediately with error.
            return new CsvProcessResult(0, 0, new List<string> { "Empty file" });
        }

        int lineNumber = 1; // Tracks the current line number (for meaningful error reporting).
        // Read each user record line by line until the end of the stream.
        while (!reader.EndOfStream)
        {
            lineNumber++;
            var line = await reader.ReadLineAsync();
            // Skip blank or whitespace-only lines.
            if (string.IsNullOrWhiteSpace(line)) continue;

            // Split the CSV line into columns by comma.
            var values = line.Split(',');
            // The minimum expected columns: Username, Email, Password, Role.
            if (values.Length < 4)
            {
                failureCount++;
                errors.Add($"Line {lineNumber}: Invalid format. Expected Username,Email,Password,Role");
                continue;
            }

            // Extract and trim individual fields.
            var username = values[0].Trim();
            var email = values[1].Trim();
            var password = values[2].Trim();
            var roleString = values[3].Trim();

            // Check for any missing required fields.
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                failureCount++;
                errors.Add($"Line {lineNumber}: Missing required fields.");
                continue;
            }

            // Attempt to parse the user role (case-insensitive).
            if (!Enum.TryParse<UserRole>(roleString, true, out var role))
            {
                failureCount++;
                errors.Add($"Line {lineNumber}: Invalid role '{roleString}'.");
                continue;
            }

            // Check if a user with this username already exists.
            var existingUser = await userRepository.GetByUsernameAsync(username);
            if (existingUser != null)
            {
                failureCount++;
                errors.Add($"Line {lineNumber}: User '{username}' already exists.");
                continue;
            }

            try
            {
                // Hash the password and attempt to create/save new user.
                var passwordHash = passwordHasher.Hash(password);
                var user = new User(username, email, passwordHash, role);
                await userRepository.AddAsync(user);
                successCount++;
            }
            catch (Exception ex)
            {
                // Catch any error while creating/saving user and report as failure.
                failureCount++;
                errors.Add($"Line {lineNumber}: Error creating user. {ex.Message}");
            }
        }

        // Persist users only if there was at least one successful creation.
        if (successCount > 0)
        {
            await unitOfWork.SaveChangesAsync();
        }

        // Return batch processing result including counts and detailed errors.
        return new CsvProcessResult(successCount, failureCount, errors);
    }
}
