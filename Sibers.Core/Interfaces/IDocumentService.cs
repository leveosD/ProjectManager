using Sibers.Core.DTOs;

namespace Sibers.Core.Interfaces;

public interface IDocumentService
{
    Task<List<ProjectDocumentDto>> GetProjectDocumentsAsync(int projectId);
    Task<ProjectDocumentDto> UploadDocumentAsync(int projectId, Stream fileStream, string fileName, string contentType, long fileSize);
    Task<(Stream stream, string contentType, string fileName)?> DownloadDocumentAsync(int documentId);
    Task DeleteDocumentAsync(int documentId);
}
