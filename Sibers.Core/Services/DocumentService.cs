using Sibers.Core.DTOs;
using Sibers.Core.Entities;
using Sibers.Core.Interfaces;

namespace Sibers.Core.Services;

public class DocumentService : IDocumentService
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly IUnitOfWork _unitOfWork;

    public DocumentService(
        IDocumentRepository documentRepository,
        IProjectRepository projectRepository,
        IFileStorageService fileStorageService,
        IUnitOfWork unitOfWork)
    {
        _documentRepository = documentRepository;
        _projectRepository = projectRepository;
        _fileStorageService = fileStorageService;
        _unitOfWork = unitOfWork;
    }

    public async Task<List<ProjectDocumentDto>> GetProjectDocumentsAsync(int projectId)
    {
        var docs = await _documentRepository.GetByProjectIdAsync(projectId);
        return docs.Select(MapToDto).ToList();
    }

    public async Task<ProjectDocumentDto> UploadDocumentAsync(
        int projectId,
        Stream fileStream,
        string fileName,
        string contentType,
        long fileSize)
    {
        return await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            if (fileSize == 0)
            {
                throw new InvalidDataException("No file was uploaded.");
            }

            var project = await _projectRepository.GetByIdAsync(projectId)
                          ?? throw new KeyNotFoundException($"Project with ID {projectId} was not found.");

            var storedFileName = await _fileStorageService.SaveFileAsync(fileStream, fileName);

            var document = new ProjectDocument
            {
                ProjectId = projectId,
                FileName = fileName,
                StoredFileName = storedFileName,
                ContentType = contentType,
                FileSize = fileSize,
                UploadedAt = DateTime.UtcNow
            };

            _documentRepository.Add(document);
            
            await _unitOfWork.SaveChangesAsync();
            
            return MapToDto(document);
        });
    }

    public async Task<(Stream stream, string contentType, string fileName)?> DownloadDocumentAsync(int documentId)
    {
        var doc = await _documentRepository.GetByIdAsync(documentId);
        if (doc == null)
        {
            return null;
        }

        var fileResult = await _fileStorageService.GetFileAsync(doc.StoredFileName, doc.FileName);
        if (!fileResult.HasValue)
        {
            return null;
        }

        return (fileResult.Value.stream, fileResult.Value.contentType, doc.FileName);
    }

    public async Task DeleteDocumentAsync(int documentId)
    {
        string? storedFileNameToDelete = null;

        await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            var doc = await _documentRepository.GetByIdAsync(documentId)
                      ?? throw new KeyNotFoundException($"Document with ID {documentId} was not found.");

            storedFileNameToDelete = doc.StoredFileName;

            _documentRepository.Delete(doc);
        }); 
        
        if (!string.IsNullOrWhiteSpace(storedFileNameToDelete))
        {
            try
            {
                await _fileStorageService.DeleteFileAsync(storedFileNameToDelete);
            }
            catch
            {
                
            }
        }
    }

    private static ProjectDocumentDto MapToDto(ProjectDocument d) => new()
    {
        Id = d.Id,
        FileName = d.FileName,
        StoredFileName = d.StoredFileName,
        ContentType = d.ContentType,
        FileSize = d.FileSize,
        UploadedAt = d.UploadedAt,
        ProjectId = d.ProjectId
    };
}
