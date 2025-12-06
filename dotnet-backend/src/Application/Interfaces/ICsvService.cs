namespace Application.Interfaces;

/// <summary>
/// Service interface for handling CSV-related operations.
/// </summary>
public interface ICsvService
{
    /// <summary>
    /// Processes a CSV stream for bulk user uploads.
    /// Reads and parses the CSV data, attempts to create users for valid rows, and reports successes and errors.
    /// </summary>
    /// <param name="csvStream">A stream representing the uploaded CSV file containing user information.</param>
    /// <returns>
    /// A <see cref="CsvProcessResult"/> object containing counts for successful and failed records, and a list of error messages.
    /// </returns>
    Task<CsvProcessResult> ProcessUserBulkUploadAsync(Stream csvStream);
}

/// <summary>
/// Represents the result of processing a CSV-based bulk user upload.
/// </summary>
/// <param name="SuccessCount">The number of successfully processed users.</param>
/// <param name="FailureCount">The number of failed user entries.</param>
/// <param name="Errors">A list containing error messages for failed operations or invalid entries.</param>
public record CsvProcessResult(int SuccessCount, int FailureCount, List<string> Errors);
