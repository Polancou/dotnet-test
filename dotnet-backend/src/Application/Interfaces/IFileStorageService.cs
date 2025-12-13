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

    /// <summary>
    /// Asynchronously retrieves a file from the storage system.
    /// </summary>
    /// <param name="path">The path or URI of the file to retrieve.</param>
    /// <returns>
    /// A tuple containing the file stream and its content type.
    /// </returns>
    Task<(Stream FileStream, string ContentType)> GetFileAsync(string path);

    /// <summary>
    /// Asynchronously deletes a file from the storage system.
    /// </summary>
    /// <param name="path">The path or URI of the file to delete.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task DeleteFileAsync(string path);
}
