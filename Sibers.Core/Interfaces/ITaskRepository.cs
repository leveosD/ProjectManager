using Sibers.Core.DTOs;
using Sibers.Core.Entities;

namespace Sibers.Core.Interfaces;

public interface ITaskRepository
{
    Task<ProjectTask?> GetByIdAsync(int id);
    Task<ProjectTask?> GetByIdWithDetailsAsync(int id);
    Task<List<ProjectTask>> GetFilteredTasksAsync(ProjectTaskFilterDto filter);
    Task<ProjectTask> AddAsync(ProjectTask task);
    Task UpdateAsync(ProjectTask task);
    Task DeleteAsync(ProjectTask task);
}
