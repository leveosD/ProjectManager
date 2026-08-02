using Sibers.Core.Enums;

namespace Sibers.Core.Entities;

/// <summary>
/// Represents a task associated with a project.
/// </summary>
public class ProjectTask
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Comment { get; set; }

    public int Priority { get; set; }

    public ProjectTaskStatus Status { get; set; } = ProjectTaskStatus.ToDo;

    public int ProjectId { get; set; }

    public Project Project { get; set; } = null!;

    public int AuthorId { get; set; }

    public Employee Author { get; set; } = null!;

    public int? ExecutorId { get; set; }

    public Employee? Executor { get; set; }
}
