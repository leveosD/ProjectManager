namespace Sibers.Core.Interfaces;

/// <summary>
/// Service for managing physical file uploads and downloads.
/// </summary>
public interface IFileStorageService
{
    Task<string> SaveFileAsync(Stream fileStream, string originalFileName);
    Task<(Stream stream, string contentType)?> GetFileAsync(string storedFileName, string originalFileName);
    Task DeleteFileAsync(string storedFileName);
}
