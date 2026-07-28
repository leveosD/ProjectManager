namespace Sibers.Core.Entities;

/// <summary>
/// Represents a software or engineering project.
/// </summary>
public class Project
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string CustomerCompany { get; set; } = string.Empty;

    public string ExecutingCompany { get; set; } = string.Empty;

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public int Priority { get; set; }

    public int ProjectManagerId { get; set; }

    public Employee ProjectManager { get; set; } = null!;

    // Navigation properties
    public ICollection<ProjectEmployee> ProjectEmployees { get; set; } = new List<ProjectEmployee>();

    public ICollection<ProjectTask> Tasks { get; set; } = new List<ProjectTask>();

    public ICollection<ProjectDocument> Documents { get; set; } = new List<ProjectDocument>();
}
