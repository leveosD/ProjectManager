using Microsoft.Extensions.Logging;
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
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<TaskService> _logger;

    public TaskService(
        ITaskRepository taskRepository,
        IProjectRepository projectRepository,
        IEmployeeRepository employeeRepository,
        IUnitOfWork unitOfWork,
        ILogger<TaskService> logger)
    {
        _taskRepository = taskRepository;
        _projectRepository = projectRepository;
        _employeeRepository = employeeRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
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
        ArgumentNullException.ThrowIfNull(dto);

        _logger.LogInformation(
            "Creating task: Title={Title}, ProjectId={ProjectId}, AuthorId={AuthorId}, ExecutorId={ExecutorId}",
            dto.Title, dto.ProjectId, dto.AuthorId, dto.ExecutorId);

        return await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            var project = await _projectRepository.GetByIdWithDetailsAsync(dto.ProjectId);
            if (project == null)
            {
                _logger.LogWarning("Task creation failed: project {ProjectId} not found.", dto.ProjectId);
                throw new KeyNotFoundException($"Project with ID {dto.ProjectId} was not found.");
            }

            var author = await _employeeRepository.GetByIdAsync(dto.AuthorId);
            if (author == null)
            {
                _logger.LogWarning("Task creation failed: author {AuthorId} not found.", dto.AuthorId);
                throw new ArgumentException($"Author employee with ID {dto.AuthorId} does not exist.");
            }

            if (dto.ExecutorId.HasValue)
            {
                _logger.LogInformation("Validating executor {ExecutorId} against project {ProjectId}.", dto.ExecutorId.Value, dto.ProjectId);
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

            _logger.LogInformation("Adding task to repository.");
            _taskRepository.Add(task);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Saving changes and retrieving task details for Id {TaskId}.", task.Id);
            var fullTask = await _taskRepository.GetByIdWithDetailsAsync(task.Id);
            if (fullTask == null)
            {
                _logger.LogError("Failed to retrieve created task details for Id {TaskId}.", task.Id);
                throw new InvalidOperationException("Failed to retrieve created task details.");
            }

            _logger.LogInformation("Task created successfully with Id {TaskId}.", fullTask.Id);
            return MapToDto(fullTask);
        });
    }

    public async Task<ProjectTaskDto> UpdateTaskAsync(int id, UpdateProjectTaskDto dto)
    {
        return await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            var task = await _taskRepository.GetByIdWithDetailsAsync(id)
                       ?? throw new KeyNotFoundException($"Task with ID {id} was not found.");

            var project = await _projectRepository.GetByIdWithDetailsAsync(task.ProjectId)
                          ?? throw new KeyNotFoundException(
                              $"Associated project with ID {task.ProjectId} was not found.");

            if (dto.ExecutorId.HasValue)
            {
                ValidateExecutorIsProjectMember(project, dto.ExecutorId.Value);
            }

            task.Title = dto.Title.Trim();
            task.Comment = string.IsNullOrWhiteSpace(dto.Comment) ? null : dto.Comment.Trim();
            task.Priority = dto.Priority;
            task.Status = dto.Status;
            task.ExecutorId = dto.ExecutorId;

            _taskRepository.Update(task);

            var fullTask = await _taskRepository.GetByIdWithDetailsAsync(id);
            return MapToDto(fullTask!);
        });
    }

    public async Task UpdateTaskStatusAsync(int id, ProjectTaskStatus status)
    {
        await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            var task = await _taskRepository.GetByIdAsync(id)
                       ?? throw new KeyNotFoundException($"Task with ID {id} was not found.");

            task.Status = status;

            _taskRepository.Update(task);
        });
    }

    public async Task AssignTaskExecutorAsync(int id, int? executorId)
    {
        await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            var task = await _taskRepository.GetByIdWithDetailsAsync(id)
                       ?? throw new KeyNotFoundException($"Task with ID {id} was not found.");

            if (executorId.HasValue)
            {
                var project = await _projectRepository.GetByIdWithDetailsAsync(task.ProjectId)
                              ?? throw new KeyNotFoundException(
                                  $"Associated project with ID {task.ProjectId} was not found.");

                ValidateExecutorIsProjectMember(project, executorId.Value);
            }

            task.ExecutorId = executorId;

            _taskRepository.Update(task);
        });
    }

    public async Task DeleteTaskAsync(int id)
    {
        await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            var task = await _taskRepository.GetByIdAsync(id)
                       ?? throw new KeyNotFoundException($"Task with ID {id} was not found.");

            _taskRepository.Delete(task);
        });
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
