using Sibers.Core.DTOs;
using Sibers.Core.Entities;

namespace Sibers.Core.Interfaces;

public interface IProjectRepository
{
    Task<Project?> GetByIdAsync(int id);
    Task<Project?> GetByIdWithDetailsAsync(int id);
    Task<List<Project>> GetFilteredProjectsAsync(ProjectFilterDto filter, string employeeId);
    void Add(Project project);
    void Update(Project project);
    void Delete(Project project);
    void SetProjectEmployees(int projectId, List<int> employeeIds);
}
