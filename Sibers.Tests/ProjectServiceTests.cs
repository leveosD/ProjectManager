using Moq;
using Sibers.Core.DTOs;
using Sibers.Core.Entities;
using Sibers.Core.Interfaces;
using Sibers.Core.Services;
using Xunit;

namespace Sibers.Tests;

public class ProjectServiceTests
{
    private readonly Mock<IProjectRepository> _projectRepoMock = new();
    private readonly Mock<IEmployeeRepository> _employeeRepoMock = new();
    private readonly ProjectService _service;

    public ProjectServiceTests()
    {
        _service = new ProjectService(_projectRepoMock.Object, _employeeRepoMock.Object);
    }

    [Fact]
    public async Task CreateProjectAsync_ShouldThrowException_WhenEndDateIsBeforeStartDate()
    {
        // Arrange
        var dto = new CreateProjectDto
        {
            Name = "Invalid Project",
            CustomerCompany = "Client Co",
            ExecutingCompany = "Dev Co",
            StartDate = DateTime.UtcNow.AddDays(10),
            EndDate = DateTime.UtcNow,
            Priority = 1,
            ProjectManagerId = 1
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateProjectAsync(dto));
        Assert.Contains("End Date cannot be earlier than Start Date", exception.Message);
    }

    [Fact]
    public async Task CreateProjectAsync_ShouldThrowException_WhenProjectManagerDoesNotExist()
    {
        // Arrange
        var dto = new CreateProjectDto
        {
            Name = "Valid Dates Project",
            CustomerCompany = "Client Co",
            ExecutingCompany = "Dev Co",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddMonths(1),
            Priority = 2,
            ProjectManagerId = 999 // Non-existent PM
        };

        _employeeRepoMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Employee?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateProjectAsync(dto));
        Assert.Contains("Project Manager with ID 999 does not exist", exception.Message);
    }

    [Fact]
    public async Task CreateProjectAsync_ShouldCreateSuccessfully_WhenValid()
    {
        // Arrange
        var dto = new CreateProjectDto
        {
            Name = "New Web App",
            CustomerCompany = "Acme Corp",
            ExecutingCompany = "Sibers",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddMonths(3),
            Priority = 5,
            ProjectManagerId = 1,
            EmployeeIds = new List<int> { 2, 3 }
        };

        var pmEmployee = new Employee { Id = 1, FirstName = "Alice", LastName = "PM", Email = "pm@sibers.com" };
        var createdProject = new Project
        {
            Id = 10,
            Name = dto.Name,
            CustomerCompany = dto.CustomerCompany,
            ExecutingCompany = dto.ExecutingCompany,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            Priority = dto.Priority,
            ProjectManagerId = dto.ProjectManagerId
        };

        _employeeRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(pmEmployee);
        _projectRepoMock.Setup(r => r.AddAsync(It.IsAny<Project>())).ReturnsAsync(createdProject);
        _projectRepoMock.Setup(r => r.GetByIdWithDetailsAsync(10)).ReturnsAsync(createdProject);

        // Act
        var result = await _service.CreateProjectAsync(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("New Web App", result.Name);
        Assert.Equal(10, result.Id);
        _projectRepoMock.Verify(r => r.SetProjectEmployeesAsync(10, It.Is<List<int>>(list => list.Count == 3 && list.Contains(1) && list.Contains(2) && list.Contains(3))), Times.Once);
    }

    [Fact]
    public async Task UpdateProjectAsync_ShouldReplacePreviousProjectManagerInTeam_WhenPmChanges()
    {
        // Arrange
        var dto = new UpdateProjectDto
        {
            Name = "Updated Project",
            CustomerCompany = "Acme Corp",
            ExecutingCompany = "Sibers",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddMonths(3),
            Priority = 5,
            ProjectManagerId = 2,
            EmployeeIds = new List<int> { 3 }
        };

        var existingProject = new Project
        {
            Id = 10,
            Name = "Old Name",
            CustomerCompany = dto.CustomerCompany,
            ExecutingCompany = dto.ExecutingCompany,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            Priority = dto.Priority,
            ProjectManagerId = 1
        };

        var newPm = new Employee { Id = 2, FirstName = "Bob", LastName = "PM", Email = "bob@sibers.com" };

        _projectRepoMock.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(existingProject);
        _employeeRepoMock.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(newPm);
        _projectRepoMock.Setup(r => r.GetByIdWithDetailsAsync(10)).ReturnsAsync(existingProject);

        // Act
        await _service.UpdateProjectAsync(10, dto);

        // Assert: team should contain new PM (2) and employee (3), but not old PM (1)
        _projectRepoMock.Verify(
            r => r.SetProjectEmployeesAsync(
                10,
                It.Is<List<int>>(list => list.Contains(2) && list.Contains(3) && !list.Contains(1))),
            Times.Once);
    }

    [Fact]
    public async Task GetProjectByIdAsync_ShouldReturnNull_WhenProjectDoesNotExist()
    {
        _projectRepoMock.Setup(r => r.GetByIdWithDetailsAsync(999)).ReturnsAsync((Project?)null);

        var result = await _service.GetProjectByIdAsync(999);

        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteProjectAsync_ShouldThrowKeyNotFoundException_WhenProjectDoesNotExist()
    {
        _projectRepoMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Project?)null);

        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.DeleteProjectAsync(999));
        Assert.Contains("Project with ID 999 was not found", exception.Message);
    }

    [Fact]
    public async Task CreateProjectAsync_ShouldAddOnlyProjectManager_WhenEmployeeIdsEmpty()
    {
        var dto = new CreateProjectDto
        {
            Name = "Solo Project",
            CustomerCompany = "Client",
            ExecutingCompany = "Sibers",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddMonths(1),
            Priority = 1,
            ProjectManagerId = 1,
            EmployeeIds = new List<int>()
        };

        var pm = new Employee { Id = 1, FirstName = "Alice", LastName = "PM", Email = "pm@sibers.com" };
        var created = new Project { Id = 20, Name = dto.Name, ProjectManagerId = 1 };

        _employeeRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(pm);
        _projectRepoMock.Setup(r => r.AddAsync(It.IsAny<Project>())).ReturnsAsync(created);
        _projectRepoMock.Setup(r => r.GetByIdWithDetailsAsync(20)).ReturnsAsync(created);

        var result = await _service.CreateProjectAsync(dto);

        Assert.NotNull(result);
        _projectRepoMock.Verify(r => r.SetProjectEmployeesAsync(20, It.Is<List<int>>(list => list.Count == 1 && list.Contains(1))), Times.Once);
    }

    [Fact]
    public async Task UpdateProjectAsync_ShouldThrowKeyNotFoundException_WhenProjectDoesNotExist()
    {
        var dto = new UpdateProjectDto
        {
            Name = "Updated",
            CustomerCompany = "Client",
            ExecutingCompany = "Sibers",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddMonths(1),
            Priority = 1,
            ProjectManagerId = 2,
            EmployeeIds = new List<int>()
        };

        _projectRepoMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Project?)null);

        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.UpdateProjectAsync(999, dto));
        Assert.Contains("Project with ID 999 was not found", exception.Message);
    }

    [Fact]
    public async Task UpdateProjectAsync_ShouldNotModifyTeam_WhenEmployeeIdsNull()
    {
        var existingProject = new Project
        {
            Id = 10,
            Name = "Old Name",
            CustomerCompany = "Client",
            ExecutingCompany = "Sibers",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddMonths(3),
            Priority = 5,
            ProjectManagerId = 1
        };

        var dto = new UpdateProjectDto
        {
            Name = "Updated Name",
            CustomerCompany = existingProject.CustomerCompany,
            ExecutingCompany = existingProject.ExecutingCompany,
            StartDate = existingProject.StartDate,
            EndDate = existingProject.EndDate,
            Priority = existingProject.Priority,
            ProjectManagerId = 1,
            EmployeeIds = null!
        };

        _projectRepoMock.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(existingProject);
        _employeeRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Employee { Id = 1 });
        _projectRepoMock.Setup(r => r.GetByIdWithDetailsAsync(10)).ReturnsAsync(existingProject);

        await _service.UpdateProjectAsync(10, dto);

        _projectRepoMock.Verify(r => r.SetProjectEmployeesAsync(It.IsAny<int>(), It.IsAny<List<int>>()), Times.Never);
    }

    [Fact]
    public async Task AddEmployeesToProjectAsync_ShouldCombineExistingAndNewEmployees()
    {
        var project = new Project
        {
            Id = 5,
            ProjectEmployees = new List<ProjectEmployee>
            {
                new() { ProjectId = 5, EmployeeId = 1 },
                new() { ProjectId = 5, EmployeeId = 2 }
            }
        };

        _projectRepoMock.Setup(r => r.GetByIdWithDetailsAsync(5)).ReturnsAsync(project);

        await _service.AddEmployeesToProjectAsync(5, new List<int> { 2, 3 });

        _projectRepoMock.Verify(
            r => r.SetProjectEmployeesAsync(
                5,
                It.Is<List<int>>(list => list.Count == 3 && list.Contains(1) && list.Contains(2) && list.Contains(3))),
            Times.Once);
    }

    [Fact]
    public async Task RemoveEmployeeFromProjectAsync_ShouldRemoveEmployeeFromTeam()
    {
        var project = new Project
        {
            Id = 5,
            ProjectEmployees = new List<ProjectEmployee>
            {
                new() { ProjectId = 5, EmployeeId = 1 },
                new() { ProjectId = 5, EmployeeId = 2 }
            }
        };

        _projectRepoMock.Setup(r => r.GetByIdWithDetailsAsync(5)).ReturnsAsync(project);

        await _service.RemoveEmployeeFromProjectAsync(5, 2);

        _projectRepoMock.Verify(
            r => r.SetProjectEmployeesAsync(
                5,
                It.Is<List<int>>(list => list.Count == 1 && list.Contains(1))),
            Times.Once);
    }
}
