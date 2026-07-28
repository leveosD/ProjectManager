namespace Sibers.Core.Entities;

/// <summary>
/// Represents an employee working in the company.
/// </summary>
public class Employee
{
    public int Id { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string? MiddleName { get; set; }

    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Foreign key referencing ASP.NET Core Identity user account (if user account exists).
    /// </summary>
    public string? UserId { get; set; }

    // Navigation properties
    public ICollection<Project> ManagedProjects { get; set; } = new List<Project>();

    public ICollection<ProjectEmployee> ProjectEmployees { get; set; } = new List<ProjectEmployee>();

    public ICollection<ProjectTask> AuthoredTasks { get; set; } = new List<ProjectTask>();

    public ICollection<ProjectTask> ExecutedTasks { get; set; } = new List<ProjectTask>();

    /// <summary>
    /// Computed helper property for full name.
    /// </summary>
    public string FullName => string.IsNullOrWhiteSpace(MiddleName)
        ? $"{LastName} {FirstName}"
        : $"{LastName} {FirstName} {MiddleName}";
}
