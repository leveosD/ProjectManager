using Sibers.Core.DTOs;
using Sibers.Core.Entities;
using Sibers.Core.Enums;
using Sibers.Core.Interfaces;

namespace Sibers.Core.Services;

public class TaskService : ITaskService
{
    private readonly ITaskRepository _taskRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly IEmployeeRepository _employeeRepository;

    public TaskService(
        ITaskRepository taskRepository,
        IProjectRepository projectRepository,
        IEmployeeRepository employeeRepository)
    {
        _taskRepository = taskRepository;
        _projectRepository = projectRepository;
        _employeeRepository = employeeRepository;
    }

    public async Task<List<ProjectTaskDto>> GetTasksAsync(ProjectTaskFilterDto filter)
    {
        var tasks = await _taskRepository.GetFilteredTasksAsync(filter);
        return tasks.Select(MapToDto).ToList();
    }

    public async Task<ProjectTaskDto?> GetTaskByIdAsync(int id)
    {
        var task = await _taskRepository.GetByIdWithDetailsAsync(id);
        return task == null ? null : MapToDto(task);
    }

    public async Task<ProjectTaskDto> CreateTaskAsync(CreateProjectTaskDto dto)
    {
        var project = await _projectRepository.GetByIdWithDetailsAsync(dto.ProjectId)
            ?? throw new KeyNotFoundException($"Project with ID {dto.ProjectId} was not found.");

        var author = await _employeeRepository.GetByIdAsync(dto.AuthorId)
            ?? throw new ArgumentException($"Author employee with ID {dto.AuthorId} does not exist.");

        if (dto.ExecutorId.HasValue)
        {
            ValidateExecutorIsProjectMember(project, dto.ExecutorId.Value);
        }

        var task = new ProjectTask
        {
            Title = dto.Title.Trim(),
            Comment = string.IsNullOrWhiteSpace(dto.Comment) ? null : dto.Comment.Trim(),
            Priority = dto.Priority,
            Status = dto.Status,
            ProjectId = dto.ProjectId,
            AuthorId = dto.AuthorId,
            ExecutorId = dto.ExecutorId
        };

        var createdTask = await _taskRepository.AddAsync(task);
        var fullTask = await _taskRepository.GetByIdWithDetailsAsync(createdTask.Id);
        return MapToDto(fullTask!);
    }

    public async Task<ProjectTaskDto> UpdateTaskAsync(int id, UpdateProjectTaskDto dto)
    {
        var task = await _taskRepository.GetByIdWithDetailsAsync(id)
            ?? throw new KeyNotFoundException($"Task with ID {id} was not found.");

        var project = await _projectRepository.GetByIdWithDetailsAsync(task.ProjectId)
            ?? throw new KeyNotFoundException($"Associated project with ID {task.ProjectId} was not found.");

        if (dto.ExecutorId.HasValue)
        {
            ValidateExecutorIsProjectMember(project, dto.ExecutorId.Value);
        }

        task.Title = dto.Title.Trim();
        task.Comment = string.IsNullOrWhiteSpace(dto.Comment) ? null : dto.Comment.Trim();
        task.Priority = dto.Priority;
        task.Status = dto.Status;
        task.ExecutorId = dto.ExecutorId;

        await _taskRepository.UpdateAsync(task);
        var fullTask = await _taskRepository.GetByIdWithDetailsAsync(id);
        return MapToDto(fullTask!);
    }

    public async Task UpdateTaskStatusAsync(int id, ProjectTaskStatus status)
    {
        var task = await _taskRepository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Task with ID {id} was not found.");

        task.Status = status;
        await _taskRepository.UpdateAsync(task);
    }

    public async Task AssignTaskExecutorAsync(int id, int? executorId)
    {
        var task = await _taskRepository.GetByIdWithDetailsAsync(id)
            ?? throw new KeyNotFoundException($"Task with ID {id} was not found.");

        if (executorId.HasValue)
        {
            var project = await _projectRepository.GetByIdWithDetailsAsync(task.ProjectId)
                ?? throw new KeyNotFoundException($"Associated project with ID {task.ProjectId} was not found.");

            ValidateExecutorIsProjectMember(project, executorId.Value);
        }

        task.ExecutorId = executorId;
        await _taskRepository.UpdateAsync(task);
    }

    public async Task DeleteTaskAsync(int id)
    {
        var task = await _taskRepository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Task with ID {id} was not found.");

        await _taskRepository.DeleteAsync(task);
    }

    private static void ValidateExecutorIsProjectMember(Project project, int executorId)
    {
        var isPm = project.ProjectManagerId == executorId;
        var isTeamMember = project.ProjectEmployees.Any(pe => pe.EmployeeId == executorId);

        if (!isPm && !isTeamMember)
        {
            throw new InvalidOperationException($"Employee with ID {executorId} is not assigned to project '{project.Name}' and cannot be assigned as task executor.");
        }
    }

    private static ProjectTaskDto MapToDto(ProjectTask t) => new()
    {
        Id = t.Id,
        Title = t.Title,
        Comment = t.Comment,
        Priority = t.Priority,
        Status = t.Status,
        ProjectId = t.ProjectId,
        ProjectName = t.Project?.Name ?? string.Empty,
        AuthorId = t.AuthorId,
        AuthorName = t.Author != null ? t.Author.FullName : string.Empty,
        ExecutorId = t.ExecutorId,
        ExecutorName = t.Executor != null ? t.Executor.FullName : null
    };
}
