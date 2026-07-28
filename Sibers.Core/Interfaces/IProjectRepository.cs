using Sibers.Core.DTOs;
using Sibers.Core.Entities;

namespace Sibers.Core.Interfaces;

public interface IProjectRepository
{
    Task<Project?> GetByIdAsync(int id);
    Task<Project?> GetByIdWithDetailsAsync(int id);
    Task<List<Project>> GetFilteredProjectsAsync(ProjectFilterDto filter);
    Task<Project> AddAsync(Project project);
    Task UpdateAsync(Project project);
    Task DeleteAsync(Project project);
    Task SetProjectEmployeesAsync(int projectId, List<int> employeeIds);
}
