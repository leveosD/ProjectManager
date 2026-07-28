namespace Sibers.Core.DTOs;

public class ProjectDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string CustomerCompany { get; set; } = string.Empty;
    public string ExecutingCompany { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int Priority { get; set; }
    public int ProjectManagerId { get; set; }
    public string ProjectManagerName { get; set; } = string.Empty;
    public string ProjectManagerEmail { get; set; } = string.Empty;
    public List<EmployeeDto> Employees { get; set; } = new();
    public List<ProjectDocumentDto> Documents { get; set; } = new();
    public int TasksCount { get; set; }
}

public class CreateProjectDto
{
    public string Name { get; set; } = string.Empty;
    public string CustomerCompany { get; set; } = string.Empty;
    public string ExecutingCompany { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int Priority { get; set; }
    public int ProjectManagerId { get; set; }
    public List<int> EmployeeIds { get; set; } = new();
}

public class UpdateProjectDto
{
    public string Name { get; set; } = string.Empty;
    public string CustomerCompany { get; set; } = string.Empty;
    public string ExecutingCompany { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int Priority { get; set; }
    public int ProjectManagerId { get; set; }
    public List<int> EmployeeIds { get; set; } = new();
}

public class ProjectFilterDto
{
    public DateTime? StartDateFrom { get; set; }
    public DateTime? StartDateTo { get; set; }
    public int? PriorityMin { get; set; }
    public int? PriorityMax { get; set; }
    public string? SearchTerm { get; set; }
    public string? SortBy { get; set; } // Name, StartDate, EndDate, Priority
    public bool SortDescending { get; set; } = false;
}
