using Moq;
using Sibers.Core.Entities;
using Sibers.Core.Interfaces;
using Sibers.Core.Services;

namespace Sibers.Tests;

public class DocumentServiceTests
{
    private readonly Mock<IDocumentRepository> _documentRepoMock = new();
    private readonly Mock<IProjectRepository> _projectRepoMock = new();
    private readonly Mock<IFileStorageService> _fileStorageMock = new();
    private readonly DocumentService _service;

    public DocumentServiceTests()
    {
        _service = new DocumentService(_documentRepoMock.Object, _projectRepoMock.Object, _fileStorageMock.Object);
    }

    [Fact]
    public async Task GetProjectDocumentsAsync_ShouldReturnMappedDocuments()
    {
        var docs = new List<ProjectDocument>
        {
            new()
            {
                Id = 1,
                ProjectId = 5,
                FileName = "spec.pdf",
                StoredFileName = "guid.pdf",
                ContentType = "application/pdf",
                FileSize = 1024,
                UploadedAt = DateTime.UtcNow
            }
        };

        _documentRepoMock.Setup(r => r.GetByProjectIdAsync(5)).ReturnsAsync(docs);

        var result = await _service.GetProjectDocumentsAsync(5);

        Assert.Single(result);
        Assert.Equal("spec.pdf", result[0].FileName);
    }

    [Fact]
    public async Task UploadDocumentAsync_ShouldThrowKeyNotFoundException_WhenProjectDoesNotExist()
    {
        _projectRepoMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Project?)null);

        var stream = new MemoryStream();
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _service.UploadDocumentAsync(999, stream, "file.txt", "text/plain", 100));

        Assert.Contains("Project with ID 999 was not found", exception.Message);
    }

    [Fact]
    public async Task UploadDocumentAsync_ShouldReturnDocumentDto_WhenUploadSucceeds()
    {
        var project = new Project { Id = 5, Name = "Sibers Portal" };
        var stream = new MemoryStream();
        var createdDoc = new ProjectDocument
        {
            Id = 1,
            ProjectId = 5,
            FileName = "report.pdf",
            StoredFileName = "stored-guid.pdf",
            ContentType = "application/pdf",
            FileSize = 2048,
            UploadedAt = DateTime.UtcNow
        };

        _projectRepoMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(project);
        _fileStorageMock.Setup(r => r.SaveFileAsync(stream, "report.pdf")).ReturnsAsync("stored-guid.pdf");
        _documentRepoMock.Setup(r => r.AddAsync(It.IsAny<ProjectDocument>())).ReturnsAsync(createdDoc);

        var result = await _service.UploadDocumentAsync(5, stream, "report.pdf", "application/pdf", 2048);

        Assert.NotNull(result);
        Assert.Equal("report.pdf", result.FileName);
        Assert.Equal("stored-guid.pdf", result.StoredFileName);
        Assert.Equal(2048, result.FileSize);
    }

    [Fact]
    public async Task DownloadDocumentAsync_ShouldReturnNull_WhenDocumentDoesNotExist()
    {
        _documentRepoMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((ProjectDocument?)null);

        var result = await _service.DownloadDocumentAsync(999);

        Assert.Null(result);
    }

    [Fact]
    public async Task DownloadDocumentAsync_ShouldReturnNull_WhenFileMissing()
    {
        var doc = new ProjectDocument
        {
            Id = 1,
            StoredFileName = "missing.pdf",
            FileName = "report.pdf",
            ContentType = "application/pdf"
        };

        _documentRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(doc);
        _fileStorageMock.Setup(r => r.GetFileAsync("missing.pdf", "report.pdf")).ReturnsAsync(((Stream stream, string contentType)?)null);

        var result = await _service.DownloadDocumentAsync(1);

        Assert.Null(result);
    }

    [Fact]
    public async Task DownloadDocumentAsync_ShouldReturnStreamAndMetadata_WhenFileExists()
    {
        var doc = new ProjectDocument
        {
            Id = 1,
            StoredFileName = "stored.pdf",
            FileName = "report.pdf",
            ContentType = "application/pdf"
        };

        var stream = new MemoryStream();
        _documentRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(doc);
        _fileStorageMock.Setup(r => r.GetFileAsync("stored.pdf", "report.pdf"))
            .ReturnsAsync((stream, "application/pdf"));

        var result = await _service.DownloadDocumentAsync(1);

        Assert.NotNull(result);
        Assert.Equal(stream, result!.Value.stream);
        Assert.Equal("report.pdf", result.Value.fileName);
        Assert.Equal("application/pdf", result.Value.contentType);
    }

    [Fact]
    public async Task DeleteDocumentAsync_ShouldThrowKeyNotFoundException_WhenDocumentDoesNotExist()
    {
        _documentRepoMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((ProjectDocument?)null);

        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.DeleteDocumentAsync(999));
        Assert.Contains("Document with ID 999 was not found", exception.Message);
    }

    [Fact]
    public async Task DeleteDocumentAsync_ShouldDeleteFileAndDocument_WhenDocumentExists()
    {
        var doc = new ProjectDocument
        {
            Id = 1,
            StoredFileName = "stored.pdf",
            FileName = "report.pdf"
        };

        _documentRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(doc);

        await _service.DeleteDocumentAsync(1);

        _fileStorageMock.Verify(r => r.DeleteFileAsync("stored.pdf"), Times.Once);
        _documentRepoMock.Verify(r => r.DeleteAsync(doc), Times.Once);
    }
}
