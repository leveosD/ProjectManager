using Sibers.Core.DTOs;
using Sibers.Core.Entities;
using Sibers.Core.Interfaces;

namespace Sibers.Core.Services;

public class ProjectService : IProjectService
{
    private readonly IProjectRepository _projectRepository;
    private readonly IEmployeeRepository _employeeRepository;

    public ProjectService(IProjectRepository projectRepository, IEmployeeRepository employeeRepository)
    {
        _projectRepository = projectRepository;
        _employeeRepository = employeeRepository;
    }

    public async Task<List<ProjectDto>> GetProjectsAsync(ProjectFilterDto filter)
    {
        var projects = await _projectRepository.GetFilteredProjectsAsync(filter);
        return projects.Select(MapToDto).ToList();
    }

    public async Task<ProjectDto?> GetProjectByIdAsync(int id)
    {
        var project = await _projectRepository.GetByIdWithDetailsAsync(id);
        return project == null ? null : MapToDto(project);
    }

    public async Task<ProjectDto> CreateProjectAsync(CreateProjectDto dto)
    {
        ValidateProjectDates(dto.StartDate, dto.EndDate);

        var pm = await _employeeRepository.GetByIdAsync(dto.ProjectManagerId)
            ?? throw new ArgumentException($"Project Manager with ID {dto.ProjectManagerId} does not exist.");

        var project = new Project
        {
            Name = dto.Name.Trim(),
            CustomerCompany = dto.CustomerCompany.Trim(),
            ExecutingCompany = dto.ExecutingCompany.Trim(),
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            Priority = dto.Priority,
            ProjectManagerId = dto.ProjectManagerId
        };

        var createdProject = await _projectRepository.AddAsync(project);

        var employeeIds = dto.EmployeeIds ?? new List<int>();
        if (!employeeIds.Contains(dto.ProjectManagerId))
        {
            employeeIds = employeeIds.Prepend(dto.ProjectManagerId).ToList();
        }

        if (employeeIds.Count > 0)
        {
            await _projectRepository.SetProjectEmployeesAsync(createdProject.Id, employeeIds);
        }

        var fullProject = await _projectRepository.GetByIdWithDetailsAsync(createdProject.Id);
        return MapToDto(fullProject!);
    }

    public async Task<ProjectDto> UpdateProjectAsync(int id, UpdateProjectDto dto)
    {
        ValidateProjectDates(dto.StartDate, dto.EndDate);

        var project = await _projectRepository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Project with ID {id} was not found.");

        var pm = await _employeeRepository.GetByIdAsync(dto.ProjectManagerId)
            ?? throw new ArgumentException($"Project Manager with ID {dto.ProjectManagerId} does not exist.");

        var previousProjectManagerId = project.ProjectManagerId;

        project.Name = dto.Name.Trim();
        project.CustomerCompany = dto.CustomerCompany.Trim();
        project.ExecutingCompany = dto.ExecutingCompany.Trim();
        project.StartDate = dto.StartDate;
        project.EndDate = dto.EndDate;
        project.Priority = dto.Priority;
        project.ProjectManagerId = dto.ProjectManagerId;

        await _projectRepository.UpdateAsync(project);

        if (dto.EmployeeIds != null)
        {
            var employeeIds = dto.EmployeeIds.ToList();
            if (!employeeIds.Contains(dto.ProjectManagerId))
            {
                employeeIds = employeeIds.Prepend(dto.ProjectManagerId).ToList();
            }

            if (previousProjectManagerId != dto.ProjectManagerId)
            {
                employeeIds = employeeIds.Where(eid => eid != previousProjectManagerId).ToList();
            }

            await _projectRepository.SetProjectEmployeesAsync(id, employeeIds);
        }

        var fullProject = await _projectRepository.GetByIdWithDetailsAsync(id);
        return MapToDto(fullProject!);
    }

    public async Task DeleteProjectAsync(int id)
    {
        var project = await _projectRepository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Project with ID {id} was not found.");

        await _projectRepository.DeleteAsync(project);
    }

    public async Task AddEmployeesToProjectAsync(int projectId, List<int> employeeIds)
    {
        var project = await _projectRepository.GetByIdWithDetailsAsync(projectId)
            ?? throw new KeyNotFoundException($"Project with ID {projectId} was not found.");

        var existingIds = project.ProjectEmployees.Select(pe => pe.EmployeeId).ToList();
        var combinedIds = existingIds.Concat(employeeIds).Distinct().ToList();

        await _projectRepository.SetProjectEmployeesAsync(projectId, combinedIds);
    }

    public async Task RemoveEmployeeFromProjectAsync(int projectId, int employeeId)
    {
        var project = await _projectRepository.GetByIdWithDetailsAsync(projectId)
            ?? throw new KeyNotFoundException($"Project with ID {projectId} was not found.");

        var currentIds = project.ProjectEmployees.Select(pe => pe.EmployeeId).Where(id => id != employeeId).ToList();

        await _projectRepository.SetProjectEmployeesAsync(projectId, currentIds);
    }

    private static void ValidateProjectDates(DateTime startDate, DateTime endDate)
    {
        if (endDate < startDate)
        {
            throw new ArgumentException("Project End Date cannot be earlier than Start Date.");
        }
    }

    private static ProjectDto MapToDto(Project p) => new()
    {
        Id = p.Id,
        Name = p.Name,
        CustomerCompany = p.CustomerCompany,
        ExecutingCompany = p.ExecutingCompany,
        StartDate = p.StartDate,
        EndDate = p.EndDate,
        Priority = p.Priority,
        ProjectManagerId = p.ProjectManagerId,
        ProjectManagerName = p.ProjectManager != null ? p.ProjectManager.FullName : string.Empty,
        ProjectManagerEmail = p.ProjectManager != null ? p.ProjectManager.Email : string.Empty,
        Employees = p.ProjectEmployees.Select(pe => new EmployeeDto
        {
            Id = pe.Employee.Id,
            FirstName = pe.Employee.FirstName,
            LastName = pe.Employee.LastName,
            MiddleName = pe.Employee.MiddleName,
            Email = pe.Employee.Email,
            FullName = pe.Employee.FullName,
            UserId = pe.Employee.UserId
        }).ToList(),
        Documents = p.Documents.Select(d => new ProjectDocumentDto
        {
            Id = d.Id,
            FileName = d.FileName,
            StoredFileName = d.StoredFileName,
            ContentType = d.ContentType,
            FileSize = d.FileSize,
            UploadedAt = d.UploadedAt,
            ProjectId = d.ProjectId
        }).ToList(),
        TasksCount = p.Tasks.Count
    };
}
