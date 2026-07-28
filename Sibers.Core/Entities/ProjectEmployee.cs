namespace Sibers.Core.Entities;

/// <summary>
/// Join entity for many-to-many relationship between Project and Employee.
/// </summary>
public class ProjectEmployee
{
    public int ProjectId { get; set; }

    public Project Project { get; set; } = null!;

    public int EmployeeId { get; set; }

    public Employee Employee { get; set; } = null!;
}
