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
    public string Title { get; set; } = string.Empty;
    public string? Comment { get; set; }
    public int Priority { get; set; }
    public ProjectTaskStatus Status { get; set; } = ProjectTaskStatus.ToDo;
    public int ProjectId { get; set; }
    public int AuthorId { get; set; }
    public int? ExecutorId { get; set; }
}

public class UpdateProjectTaskDto
{
    public string Title { get; set; } = string.Empty;
    public string? Comment { get; set; }
    public int Priority { get; set; }
    public ProjectTaskStatus Status { get; set; }
    public int? ExecutorId { get; set; }
}

public class ProjectTaskFilterDto
{
    public int? ProjectId { get; set; }
    public ProjectTaskStatus? Status { get; set; }
    public int? AuthorId { get; set; }
    public int? ExecutorId { get; set; }
    public string? SearchTerm { get; set; }
    public string? SortBy { get; set; } // Title, Priority, Status
    public bool SortDescending { get; set; } = false;
}
