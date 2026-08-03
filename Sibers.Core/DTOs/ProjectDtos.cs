using System.ComponentModel.DataAnnotations;
using Sibers.Core.Enums;

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
    [Required(ErrorMessage = "Project name is required.")]
    [StringLength(200, ErrorMessage = "Project name cannot exceed 200 characters.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Customer company is required.")]
    [StringLength(200, ErrorMessage = "Customer company cannot exceed 200 characters.")]
    public string CustomerCompany { get; set; } = string.Empty;

    [Required(ErrorMessage = "Executing company is required.")]
    [StringLength(200, ErrorMessage = "Executing company cannot exceed 200 characters.")]
    public string ExecutingCompany { get; set; } = string.Empty;

    [Required(ErrorMessage = "Start date is required.")]
    public DateTime StartDate { get; set; }

    [Required(ErrorMessage = "End date is required.")]
    public DateTime EndDate { get; set; }

    [Required(ErrorMessage = "Priority is required.")]
    public int Priority { get; set; }

    [Required(ErrorMessage = "Project manager is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "Project manager must be a valid employee.")]
    public int ProjectManagerId { get; set; }

    public List<int> EmployeeIds { get; set; } = new();
}

public class UpdateProjectDto
{
    [Required(ErrorMessage = "Project name is required.")]
    [StringLength(200, ErrorMessage = "Project name cannot exceed 200 characters.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Customer company is required.")]
    [StringLength(200, ErrorMessage = "Customer company cannot exceed 200 characters.")]
    public string CustomerCompany { get; set; } = string.Empty;

    [Required(ErrorMessage = "Executing company is required.")]
    [StringLength(200, ErrorMessage = "Executing company cannot exceed 200 characters.")]
    public string ExecutingCompany { get; set; } = string.Empty;

    [Required(ErrorMessage = "Start date is required.")]
    public DateTime StartDate { get; set; }

    [Required(ErrorMessage = "End date is required.")]
    public DateTime EndDate { get; set; }

    [Required(ErrorMessage = "Priority is required.")]
    public int Priority { get; set; }

    [Required(ErrorMessage = "Project manager is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "Project manager must be a valid employee.")]
    public int ProjectManagerId { get; set; }

    public List<int> EmployeeIds { get; set; } = new();
}

public class ProjectFilterDto
{
    public DateTime? StartDateFrom { get; set; }

    public DateTime? StartDateTo { get; set; }

    public int? PriorityMin { get; set; }

    public int? PriorityMax { get; set; }

    [StringLength(2000, ErrorMessage = "Search term cannot exceed 2000 characters.")]
    public string? SearchTerm { get; set; }

    [StringLength(50, ErrorMessage = "Sort field cannot exceed 50 characters.")]
    public string? SortBy { get; set; } // Name, StartDate, EndDate, Priority

    public bool SortDescending { get; set; } = false;
}
