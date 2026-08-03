using System.ComponentModel.DataAnnotations;
using Sibers.Core.Enums;

namespace Sibers.Core.DTOs;

public class ProjectTaskDto
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Comment { get; set; }

    public int Priority { get; set; }

    public ProjectTaskStatus Status { get; set; }

    public int ProjectId { get; set; }

    public string ProjectName { get; set; } = string.Empty;

    public int AuthorId { get; set; }

    public string AuthorName { get; set; } = string.Empty;

    public int? ExecutorId { get; set; }

    public string? ExecutorName { get; set; }
}

public class CreateProjectTaskDto
{
    [Required(ErrorMessage = "Task title is required.")]
    [StringLength(200, ErrorMessage = "Task title cannot exceed 200 characters.")]
    public string Title { get; set; } = string.Empty;

    [StringLength(2000, ErrorMessage = "Comment cannot exceed 2000 characters.")]
    public string? Comment { get; set; }

    [Required(ErrorMessage = "Priority is required.")]
    public int Priority { get; set; }

    [Required(ErrorMessage = "Status is required.")]
    [EnumDataType(typeof(ProjectTaskStatus), ErrorMessage = "Invalid task status.")]
    public ProjectTaskStatus Status { get; set; } = ProjectTaskStatus.ToDo;

    [Required(ErrorMessage = "Project is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "Project must be a valid project.")]
    public int ProjectId { get; set; }

    [Required(ErrorMessage = "Author is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "Author must be a valid employee.")]
    public int AuthorId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Executor must be a valid employee.")]
    public int? ExecutorId { get; set; }
}

public class UpdateProjectTaskDto
{
    [Required(ErrorMessage = "Task title is required.")]
    [StringLength(200, ErrorMessage = "Task title cannot exceed 200 characters.")]
    public string Title { get; set; } = string.Empty;

    [StringLength(2000, ErrorMessage = "Comment cannot exceed 2000 characters.")]
    public string? Comment { get; set; }

    [Required(ErrorMessage = "Priority is required.")]
    public int Priority { get; set; }

    [Required(ErrorMessage = "Status is required.")]
    [EnumDataType(typeof(ProjectTaskStatus), ErrorMessage = "Invalid task status.")]
    public ProjectTaskStatus Status { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Executor must be a valid employee.")]
    public int? ExecutorId { get; set; }
}

public class ProjectTaskFilterDto
{
    [Range(1, int.MaxValue, ErrorMessage = "Project must be a valid project.")]
    public int? ProjectId { get; set; }

    public ProjectTaskStatus? Status { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Author must be a valid employee.")]
    public int? AuthorId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Executor must be a valid employee.")]
    public int? ExecutorId { get; set; }

    [StringLength(2000, ErrorMessage = "Search term cannot exceed 2000 characters.")]
    public string? SearchTerm { get; set; }

    [StringLength(50, ErrorMessage = "Sort field cannot exceed 50 characters.")]
    public string? SortBy { get; set; } // Title, Priority, Status

    public bool SortDescending { get; set; } = false;
}
