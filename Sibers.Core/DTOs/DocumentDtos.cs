using System.ComponentModel.DataAnnotations;

namespace Sibers.Core.DTOs;

public class ProjectDocumentDto
{
    public int Id { get; set; }

    public string FileName { get; set; } = string.Empty;

    public string StoredFileName { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;

    public long FileSize { get; set; }

    public DateTime UploadedAt { get; set; }

    public int ProjectId { get; set; }
}

public class CreateProjectDocumentDto
{
    [Required(ErrorMessage = "File name is required.")]
    [StringLength(255, ErrorMessage = "File name cannot exceed 255 characters.")]
    public string FileName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Stored file name is required.")]
    [StringLength(255, ErrorMessage = "Stored file name cannot exceed 255 characters.")]
    public string StoredFileName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Content type is required.")]
    [StringLength(255, ErrorMessage = "Content type cannot exceed 255 characters.")]
    public string ContentType { get; set; } = string.Empty;

    [Required(ErrorMessage = "File size is required.")]
    [Range(0, long.MaxValue, ErrorMessage = "File size must be a non-negative value.")]
    public long FileSize { get; set; }

    [Required(ErrorMessage = "Project is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "Project must be a valid project.")]
    public int ProjectId { get; set; }
}
