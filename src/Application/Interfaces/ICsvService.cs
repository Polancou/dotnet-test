using Application.DTOs;

namespace Application.Interfaces;

public interface ICsvService
{
    Task<CsvProcessResult> ProcessUserBulkUploadAsync(Stream csvStream);
}

public record CsvProcessResult(int SuccessCount, int FailureCount, List<string> Errors);
