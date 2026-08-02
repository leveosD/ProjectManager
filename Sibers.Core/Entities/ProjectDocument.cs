namespace Sibers.Core.Entities;

/// <summary>
/// Represents a file document attached to a project.
/// </summary>
public class ProjectDocument
{
    public int Id { get; set; }

    public string FileName { get; set; } = string.Empty;

    public string StoredFileName { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;

    public long FileSize { get; set; }

    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    public int ProjectId { get; set; }

    public Project Project { get; set; } = null!;
}
