using Sibers.Core.Interfaces;

namespace Sibers.Infrastructure.Services;

public class FileStorageService : IFileStorageService
{
    private readonly string _uploadsFolderPath;

    public FileStorageService(string? uploadsPath = null)
    {
        _uploadsFolderPath = uploadsPath ?? Path.Combine(Directory.GetCurrentDirectory(), "Uploads");

        if (!Directory.Exists(_uploadsFolderPath))
        {
            Directory.CreateDirectory(_uploadsFolderPath);
        }
    }

    public async Task<string> SaveFileAsync(Stream fileStream, string originalFileName)
    {
        var fileExtension = Path.GetExtension(originalFileName);
        var storedFileName = $"{Guid.NewGuid()}{fileExtension}";
        var filePath = Path.Combine(_uploadsFolderPath, storedFileName);

        using (var outputStream = new FileStream(filePath, FileMode.Create))
        {
            await fileStream.CopyToAsync(outputStream);
        }

        return storedFileName;
    }

    public Task<(Stream stream, string contentType)?> GetFileAsync(string storedFileName, string originalFileName)
    {
        var filePath = Path.Combine(_uploadsFolderPath, storedFileName);

        if (!File.Exists(filePath))
        {
            return Task.FromResult<(Stream stream, string contentType)?>(null);
        }

        var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        var contentType = GetContentType(originalFileName);

        return Task.FromResult<(Stream stream, string contentType)?>((stream, contentType));
    }

    public Task DeleteFileAsync(string storedFileName)
    {
        var filePath = Path.Combine(_uploadsFolderPath, storedFileName);

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        return Task.CompletedTask;
    }

    private static string GetContentType(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension switch
        {
            ".pdf" => "application/pdf",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".xls" => "application/vnd.ms-excel",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".png" => "image/png",
            ".jpg" => "image/jpeg",
            ".jpeg" => "image/jpeg",
            ".txt" => "text/plain",
            ".zip" => "application/zip",
            _ => "application/octet-stream"
        };
    }
}
