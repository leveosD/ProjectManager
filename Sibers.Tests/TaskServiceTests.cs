using Moq;
using Sibers.Core.DTOs;
using Sibers.Core.Entities;
using Sibers.Core.Enums;
using Sibers.Core.Interfaces;
using Sibers.Core.Services;
using Xunit;

namespace Sibers.Tests;

public class TaskServiceTests
{
    private readonly Mock<ITaskRepository> _taskRepoMock = new();
    private readonly Mock<IProjectRepository> _projectRepoMock = new();
    private readonly Mock<IEmployeeRepository> _employeeRepoMock = new();
    private readonly TaskService _service;

    public TaskServiceTests()
    {
        _service = new TaskService(_taskRepoMock.Object, _projectRepoMock.Object, _employeeRepoMock.Object);
    }

    [Fact]
    public async Task CreateTaskAsync_ShouldThrowException_WhenExecutorIsNotProjectMember()
    {
        // Arrange
        var dto = new CreateProjectTaskDto
        {
            Title = "Implement Feature",
            Priority = 1,
            Status = ProjectTaskStatus.ToDo,
            ProjectId = 5,
            AuthorId = 1,
            ExecutorId = 99 // Employee 99 is NOT in project team or PM
        };

        var project = new Project
        {
            Id = 5,
            Name = "Sibers Portal",
            ProjectManagerId = 1, // PM is employee 1
            ProjectEmployees = new List<ProjectEmployee>
            {
                new() { ProjectId = 5, EmployeeId = 2 } // Team member is employee 2
            }
        };

        var author = new Employee { Id = 1, FirstName = "Alice", LastName = "Author" };

        _projectRepoMock.Setup(r => r.GetByIdWithDetailsAsync(5)).ReturnsAsync(project);
        _employeeRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(author);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateTaskAsync(dto));
        Assert.Contains("Employee with ID 99 is not assigned to project 'Sibers Portal'", exception.Message);
    }

    [Fact]
    public async Task CreateTaskAsync_ShouldSucceed_WhenExecutorIsProjectMember()
    {
        // Arrange
        var dto = new CreateProjectTaskDto
        {
            Title = "Fix Bug #101",
            Priority = 3,
            Status = ProjectTaskStatus.InProgress,
            ProjectId = 5,
            AuthorId = 1,
            ExecutorId = 2 // Employee 2 IS in project team
        };

        var project = new Project
        {
            Id = 5,
            Name = "Sibers Portal",
            ProjectManagerId = 1,
            ProjectEmployees = new List<ProjectEmployee>
            {
                new() { ProjectId = 5, EmployeeId = 2 }
            }
        };

        var author = new Employee { Id = 1, FirstName = "Alice", LastName = "Author" };
        var executor = new Employee { Id = 2, FirstName = "Bob", LastName = "Executor" };

        var createdTask = new ProjectTask
        {
            Id = 101,
            Title = dto.Title,
            Priority = dto.Priority,
            Status = dto.Status,
            ProjectId = dto.ProjectId,
            AuthorId = dto.AuthorId,
            ExecutorId = dto.ExecutorId,
            Project = project,
            Author = author,
            Executor = executor
        };

        _projectRepoMock.Setup(r => r.GetByIdWithDetailsAsync(5)).ReturnsAsync(project);
        _employeeRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(author);
        _taskRepoMock.Setup(r => r.AddAsync(It.IsAny<ProjectTask>())).ReturnsAsync(createdTask);
        _taskRepoMock.Setup(r => r.GetByIdWithDetailsAsync(101)).ReturnsAsync(createdTask);

        // Act
        var result = await _service.CreateTaskAsync(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Fix Bug #101", result.Title);
        Assert.Equal(2, result.ExecutorId);
    }

    [Fact]
    public async Task UpdateTaskStatusAsync_ShouldUpdateStatus_WhenTaskExists()
    {
        // Arrange
        var task = new ProjectTask
        {
            Id = 10,
            Title = "Test Task",
            Status = ProjectTaskStatus.ToDo
        };

        _taskRepoMock.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(task);

        // Act
        await _service.UpdateTaskStatusAsync(10, ProjectTaskStatus.Done);

        // Assert
        Assert.Equal(ProjectTaskStatus.Done, task.Status);
        _taskRepoMock.Verify(r => r.UpdateAsync(task), Times.Once);
    }

    [Fact]
    public async Task CreateTaskAsync_ShouldThrowKeyNotFoundException_WhenProjectDoesNotExist()
    {
        var dto = new CreateProjectTaskDto
        {
            Title = "Task",
            Priority = 1,
            Status = ProjectTaskStatus.ToDo,
            ProjectId = 999,
            AuthorId = 1
        };

        _projectRepoMock.Setup(r => r.GetByIdWithDetailsAsync(999)).ReturnsAsync((Project?)null);

        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.CreateTaskAsync(dto));
        Assert.Contains("Project with ID 999 was not found", exception.Message);
    }

    [Fact]
    public async Task CreateTaskAsync_ShouldThrowArgumentException_WhenAuthorDoesNotExist()
    {
        var dto = new CreateProjectTaskDto
        {
            Title = "Task",
            Priority = 1,
            Status = ProjectTaskStatus.ToDo,
            ProjectId = 5,
            AuthorId = 999
        };

        var project = new Project
        {
            Id = 5,
            Name = "Sibers Portal",
            ProjectManagerId = 1,
            ProjectEmployees = new List<ProjectEmployee>()
        };

        _projectRepoMock.Setup(r => r.GetByIdWithDetailsAsync(5)).ReturnsAsync(project);
        _employeeRepoMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Employee?)null);

        var exception = await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateTaskAsync(dto));
        Assert.Contains("Author employee with ID 999 does not exist", exception.Message);
    }

    [Fact]
    public async Task CreateTaskAsync_ShouldSucceed_WhenExecutorIsNull()
    {
        var dto = new CreateProjectTaskDto
        {
            Title = "Unassigned Task",
            Priority = 2,
            Status = ProjectTaskStatus.ToDo,
            ProjectId = 5,
            AuthorId = 1,
            ExecutorId = null
        };

        var project = new Project
        {
            Id = 5,
            Name = "Sibers Portal",
            ProjectManagerId = 1,
            ProjectEmployees = new List<ProjectEmployee>()
        };
        var author = new Employee { Id = 1, FirstName = "Alice", LastName = "Author" };
        var created = new ProjectTask { Id = 200, Title = dto.Title, ProjectId = 5, AuthorId = 1 };

        _projectRepoMock.Setup(r => r.GetByIdWithDetailsAsync(5)).ReturnsAsync(project);
        _employeeRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(author);
        _taskRepoMock.Setup(r => r.AddAsync(It.IsAny<ProjectTask>())).ReturnsAsync(created);
        _taskRepoMock.Setup(r => r.GetByIdWithDetailsAsync(200)).ReturnsAsync(created);

        var result = await _service.CreateTaskAsync(dto);

        Assert.NotNull(result);
        Assert.Null(result.ExecutorId);
    }

    [Fact]
    public async Task UpdateTaskStatusAsync_ShouldThrowKeyNotFoundException_WhenTaskDoesNotExist()
    {
        _taskRepoMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((ProjectTask?)null);

        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.UpdateTaskStatusAsync(999, ProjectTaskStatus.Done));
        Assert.Contains("Task with ID 999 was not found", exception.Message);
    }

    [Fact]
    public async Task DeleteTaskAsync_ShouldDelete_WhenTaskExists()
    {
        var task = new ProjectTask { Id = 10, Title = "To Delete" };
        _taskRepoMock.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(task);

        await _service.DeleteTaskAsync(10);

        _taskRepoMock.Verify(r => r.DeleteAsync(task), Times.Once);
    }

    [Fact]
    public async Task AssignTaskExecutorAsync_ShouldSetExecutor_WhenEmployeeIsProjectMember()
    {
        var task = new ProjectTask
        {
            Id = 10,
            ProjectId = 5,
            ExecutorId = null
        };

        var project = new Project
        {
            Id = 5,
            Name = "Sibers Portal",
            ProjectManagerId = 1,
            ProjectEmployees = new List<ProjectEmployee>
            {
                new() { ProjectId = 5, EmployeeId = 2 }
            }
        };

        _taskRepoMock.Setup(r => r.GetByIdWithDetailsAsync(10)).ReturnsAsync(task);
        _projectRepoMock.Setup(r => r.GetByIdWithDetailsAsync(5)).ReturnsAsync(project);

        await _service.AssignTaskExecutorAsync(10, 2);

        Assert.Equal(2, task.ExecutorId);
        _taskRepoMock.Verify(r => r.UpdateAsync(task), Times.Once);
    }

    [Fact]
    public async Task AssignTaskExecutorAsync_ShouldThrow_WhenEmployeeIsNotProjectMember()
    {
        var task = new ProjectTask
        {
            Id = 10,
            ProjectId = 5
        };

        var project = new Project
        {
            Id = 5,
            Name = "Sibers Portal",
            ProjectManagerId = 1,
            ProjectEmployees = new List<ProjectEmployee>()
        };

        _taskRepoMock.Setup(r => r.GetByIdWithDetailsAsync(10)).ReturnsAsync(task);
        _projectRepoMock.Setup(r => r.GetByIdWithDetailsAsync(5)).ReturnsAsync(project);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.AssignTaskExecutorAsync(10, 99));
        Assert.Contains("Employee with ID 99 is not assigned to project 'Sibers Portal'", exception.Message);
    }
}
