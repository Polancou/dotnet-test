using Application.Interfaces;

namespace Infrastructure.Services;

/// <summary>
/// Service for managing file storage on the local filesystem.
/// Handles file saving and ensures the upload directory exists.
/// </summary>
public class FileStorageService : IFileStorageService
{
    /// <summary>
    /// The directory where uploaded files are stored.
    /// </summary>
    private readonly string _uploadDirectory;

    /// <summary>
    /// Initializes the file storage service.
    /// Ensures the upload directory exists or creates it if missing.
    /// </summary>
    public FileStorageService()
    {
        // In a real application, consider sourcing this path from configuration for flexibility.
        _uploadDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

        // Create the uploads directory if it does not already exist.
        if (!Directory.Exists(_uploadDirectory))
        {
            Directory.CreateDirectory(_uploadDirectory);
        }
    }

    /// <summary>
    /// Saves the provided file stream to the uploads directory with a unique file name.
    /// </summary>
    /// <param name="fileStream">The stream containing the file's contents.</param>
    /// <param name="fileName">The original name of the uploaded file.</param>
    /// <returns>
    /// The full path to the saved file on the filesystem.
    /// </returns>
    public async Task<string> SaveFileAsync(Stream fileStream, string fileName)
    {
        // Prefix the file name with a GUID to guarantee uniqueness and avoid overwriting existing files.
        var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
        var filePath = Path.Combine(_uploadDirectory, uniqueFileName);

        // Create and open the file for writing, then copy the contents from the input stream.
        using (var fileStreamOutput = new FileStream(filePath, FileMode.Create))
        {
            await fileStream.CopyToAsync(fileStreamOutput);
        }

        // Return the full file path to the saved file.
        return filePath;
    }
}
