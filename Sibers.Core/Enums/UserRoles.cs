namespace Sibers.Core.Enums;

/// <summary>
/// System roles used for access control.
/// </summary>
public static class UserRoles
{
    public const string Director = "Director";
    public const string ProjectManager = "ProjectManager";
    public const string Employee = "Employee";
    
    public static readonly IReadOnlySet<string> All = new HashSet<string>()
    {
        Director,
        ProjectManager,
        Employee
    };
}
