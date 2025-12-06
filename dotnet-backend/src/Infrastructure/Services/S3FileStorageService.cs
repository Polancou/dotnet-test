using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

/// <summary>
/// Service for managing file storage on AWS S3.
/// Handles file saving to S3 buckets with proper content type and encryption.
/// </summary>
public class S3FileStorageService : IFileStorageService, IDisposable
{
    private readonly IAmazonS3 _s3Client;
    private readonly string _bucketName;
    private readonly ILogger<S3FileStorageService>? _logger;

    /// <summary>
    /// Initializes the S3 file storage service with configuration from IConfiguration.
    /// </summary>
    /// <param name="configuration">Application configuration containing AWS settings.</param>
    /// <param name="logger">Optional logger for error tracking.</param>
    /// <exception cref="ArgumentException">Thrown if required AWS configuration is missing.</exception>
    public S3FileStorageService(IConfiguration configuration, ILogger<S3FileStorageService>? logger = null)
    {
        _logger = logger;

        var accessKeyId = configuration["Aws:AccessKeyId"];
        var secretAccessKey = configuration["Aws:SecretAccessKey"];
        var region = configuration["Aws:Region"] ?? "us-east-1";
        _bucketName = configuration["Aws:S3BucketName"] ??
                      throw new ArgumentException("Aws:S3BucketName is required for S3 storage");

        if (string.IsNullOrWhiteSpace(accessKeyId) || string.IsNullOrWhiteSpace(secretAccessKey))
        {
            throw new ArgumentException("AWS AccessKeyId and SecretAccessKey must be configured for S3 storage");
        }

        // Initialize S3 client with credentials
        var regionEndpoint = RegionEndpoint.GetBySystemName(region);
        _s3Client = new AmazonS3Client(accessKeyId, secretAccessKey, regionEndpoint);

        // Verify bucket exists and is accessible
        VerifyBucketAccessAsync().Wait();
    }

    /// <summary>
    /// Initializes the S3 file storage service with explicit credentials (useful for testing).
    /// </summary>
    public S3FileStorageService(string accessKeyId, string secretAccessKey, string region, string bucketName,
        ILogger<S3FileStorageService>? logger = null)
    {
        _logger = logger;
        _bucketName = bucketName ?? throw new ArgumentNullException(nameof(bucketName));

        if (string.IsNullOrWhiteSpace(accessKeyId) || string.IsNullOrWhiteSpace(secretAccessKey))
        {
            throw new ArgumentException("AWS AccessKeyId and SecretAccessKey are required");
        }

        var regionEndpoint = RegionEndpoint.GetBySystemName(region);
        _s3Client = new AmazonS3Client(accessKeyId, secretAccessKey, regionEndpoint);

        VerifyBucketAccessAsync().Wait();
    }

    /// <summary>
    /// Asynchronously saves a file to S3 with a unique file name.
    /// </summary>
    /// <param name="fileStream">The stream containing the file's contents.</param>
    /// <param name="fileName">The original name of the uploaded file.</param>
    /// <returns>
    /// The S3 object key (path) of the stored file, which can be used to construct the full S3 URL.
    /// Format: s3://bucket-name/unique-file-name
    /// </returns>
    public async Task<string> SaveFileAsync(Stream fileStream, string fileName)
    {
        // Prefix the file name with a GUID to guarantee uniqueness
        var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";

        // Determine content type from file extension
        var contentType = GetContentType(fileName);

        try
        {
            var putRequest = new PutObjectRequest
            {
                BucketName = _bucketName,
                Key = uniqueFileName,
                InputStream = fileStream,
                ContentType = contentType,
                ServerSideEncryptionMethod = ServerSideEncryptionMethod.AES256
            };

            await _s3Client.PutObjectAsync(putRequest);

            // Return S3 object key (can be used to construct URL)
            return $"s3://{_bucketName}/{uniqueFileName}";
        }
        catch (AmazonS3Exception ex)
        {
            _logger?.LogError(ex, "Error uploading file to S3: {FileName}", fileName);
            throw new Exception($"Failed to upload file to S3: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Unexpected error uploading file to S3: {FileName}", fileName);
            throw;
        }
    }

    /// <summary>
    /// Verifies that the configured S3 bucket exists and is accessible.
    /// </summary>
    private async Task VerifyBucketAccessAsync()
    {
        try
        {
            await _s3Client.GetBucketLocationAsync(_bucketName);
        }
        catch (AmazonS3Exception ex)
        {
            if (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                throw new ArgumentException($"S3 bucket '{_bucketName}' does not exist");
            }
            else if (ex.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                throw new ArgumentException($"Access denied to S3 bucket '{_bucketName}'. Check your AWS credentials.");
            }
            else
            {
                throw new ArgumentException($"Error accessing S3 bucket '{_bucketName}': {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Determines the MIME content type based on file extension.
    /// </summary>
    /// <param name="fileName">The file name.</param>
    /// <returns>The MIME content type.</returns>
    private static string GetContentType(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension switch
        {
            ".pdf" => "application/pdf",
            ".txt" => "text/plain",
            ".csv" => "text/csv",
            ".json" => "application/json",
            ".xml" => "application/xml",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".xls" => "application/vnd.ms-excel",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            _ => "application/octet-stream"
        };
    }

    /// <summary>
    /// Disposes the S3 client.
    /// </summary>
    public void Dispose()
    {
        _s3Client.Dispose();
    }
}

