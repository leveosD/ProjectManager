using Sibers.Core.DTOs;
using Sibers.Core.Enums;

namespace Sibers.Core.Interfaces;

public interface ITaskService
{
    Task<List<ProjectTaskDto>> GetTasksAsync(ProjectTaskFilterDto filter);
    Task<ProjectTaskDto?> GetTaskByIdAsync(int id);
    Task<ProjectTaskDto> CreateTaskAsync(CreateProjectTaskDto dto);
    Task<ProjectTaskDto> UpdateTaskAsync(int id, UpdateProjectTaskDto dto);
    Task UpdateTaskStatusAsync(int id, ProjectTaskStatus status);
    Task AssignTaskExecutorAsync(int id, int? executorId);
    Task DeleteTaskAsync(int id);
}
