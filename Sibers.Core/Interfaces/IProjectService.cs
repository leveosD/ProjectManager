using Sibers.Core.DTOs;

namespace Sibers.Core.Interfaces;

public interface IProjectService
{
    Task<List<ProjectDto>> GetProjectsAsync(ProjectFilterDto filter);
    Task<ProjectDto?> GetProjectByIdAsync(int id);
    Task<ProjectDto> CreateProjectAsync(CreateProjectDto dto);
    Task<ProjectDto> UpdateProjectAsync(int id, UpdateProjectDto dto);
    Task DeleteProjectAsync(int id);
    Task AddEmployeesToProjectAsync(int projectId, List<int> employeeIds);
    Task RemoveEmployeeFromProjectAsync(int projectId, int employeeId);
}
