namespace Application.Interfaces;

/// <summary>
/// Service interface for file storage operations.
/// Defines methods to store files, typically used for persistent document/file upload and management.
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    /// Asynchronously saves a file to the storage system.
    /// </summary>
    /// <param name="fileStream">A stream representing the file content to save.</param>
    /// <param name="fileName">The name under which the file should be stored.</param>
    /// <returns>
    /// A task that represents the asynchronous operation, containing the path or URI of the stored file as a string.
    /// </returns>
    Task<string> SaveFileAsync(Stream fileStream, string fileName);
}
