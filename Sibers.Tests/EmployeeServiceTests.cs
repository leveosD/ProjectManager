using Moq;
using Sibers.Core.DTOs;
using Sibers.Core.Entities;
using Sibers.Core.Interfaces;
using Sibers.Core.Services;

namespace Sibers.Tests;

public class EmployeeServiceTests
{
    private readonly Mock<IEmployeeRepository> _employeeRepoMock = new();
    private readonly Mock<IEmployeeAccountService> _employeeAccountServiceMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly EmployeeService _service;

    public EmployeeServiceTests()
    {
        _unitOfWorkMock
            .Setup(u => u.ExecuteInTransactionAsync(It.IsAny<Func<Task<EmployeeDto>>>()))
            .Returns<Func<Task<EmployeeDto>>>(f => f());

        _unitOfWorkMock
            .Setup(u => u.ExecuteInTransactionAsync(It.IsAny<Func<Task>>()))
            .Returns<Func<Task>>(f => f());

        _unitOfWorkMock
            .Setup(u => u.CommitTransactionAsync())
            .Returns(Task.CompletedTask);

        _service = new EmployeeService(
            _employeeRepoMock.Object,
            _employeeAccountServiceMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task GetAllEmployeesAsync_ShouldReturnMappedEmployees()
    {
        var employees = new List<Employee>
        {
            new() { Id = 1, FirstName = "Alice", LastName = "Smith", Email = "alice@sibers.com" },
            new() { Id = 2, FirstName = "Bob", LastName = "Jones", Email = "bob@sibers.com" }
        };

        _employeeRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(employees);
        _employeeAccountServiceMock
            .Setup(s => s.GetRoleByEmployeeIdAsync(It.IsAny<int>()))
            .ReturnsAsync((string?)null);

        var result = await _service.GetAllEmployeesAsync();

        Assert.Equal(2, result.Count);
        Assert.Equal("Smith Alice", result[0].FullName);
        Assert.Equal("Jones Bob", result[1].FullName);
    }

    [Fact]
    public async Task SearchEmployeesAsync_ShouldReturnResultsFromRepository()
    {
        var employees = new List<Employee>
        {
            new() { Id = 1, FirstName = "Alice", LastName = "Smith", Email = "alice@sibers.com" }
        };

        _employeeRepoMock.Setup(r => r.SearchEmployeesAsync("ali", null)).ReturnsAsync(employees);
        _employeeAccountServiceMock
            .Setup(s => s.GetRoleByEmployeeIdAsync(It.IsAny<int>()))
            .ReturnsAsync((string?)null);

        var result = await _service.SearchEmployeesAsync("ali");

        Assert.Single(result);
        Assert.Equal("Alice", result[0].FirstName);
    }

    [Fact]
    public async Task GetEmployeeByIdAsync_ShouldReturnEmployee_WhenExists()
    {
        var employee = new Employee { Id = 1, FirstName = "Alice", LastName = "Smith", Email = "alice@sibers.com" };
        _employeeRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(employee);
        _employeeAccountServiceMock.Setup(s => s.GetRoleByEmployeeIdAsync(1)).ReturnsAsync("Employee");

        var result = await _service.GetEmployeeByIdAsync(1);

        Assert.NotNull(result);
        Assert.Equal("Smith Alice", result!.FullName);
        Assert.Equal("Employee", result.Role);
    }

    [Fact]
    public async Task GetEmployeeByIdAsync_ShouldReturnNull_WhenNotExists()
    {
        _employeeRepoMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Employee?)null);

        var result = await _service.GetEmployeeByIdAsync(999);

        Assert.Null(result);
    }

    [Fact]
    public async Task CreateEmployeeAsync_ShouldNormalizeEmailAndTrimNames()
    {
        var dto = new CreateEmployeeDto
        {
            FirstName = "  Alice  ",
            LastName = "  Smith  ",
            MiddleName = "  Marie  ",
            Email = "  Alice@Sibers.COM  ",
            Password = "password123"
        };

        Employee? captured = null;
        _employeeRepoMock.Setup(r => r.Add(It.IsAny<Employee>()))
            .Callback<Employee>(e => captured = e);

        _employeeAccountServiceMock
            .Setup(s => s.CreateAccountAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>()))
            .Returns(Task.CompletedTask);

        _employeeAccountServiceMock
            .Setup(s => s.GetRoleByEmployeeIdAsync(1))
            .ReturnsAsync((string?)null);

        await _service.CreateEmployeeAsync(dto);

        Assert.NotNull(captured);
        Assert.Equal("Alice", captured!.FirstName);
        Assert.Equal("Smith", captured.LastName);
        Assert.Equal("Marie", captured.MiddleName);
        Assert.Equal("alice@sibers.com", captured.Email);
    }

    [Fact]
    public async Task CreateEmployeeAsync_ShouldSetMiddleNameToNull_WhenEmpty()
    {
        var dto = new CreateEmployeeDto
        {
            FirstName = "Alice",
            LastName = "Smith",
            MiddleName = "   ",
            Email = "alice@sibers.com",
            Password = "password123"
        };

        Employee? captured = null;
        _employeeRepoMock.Setup(r => r.Add(It.IsAny<Employee>()))
            .Callback<Employee>(e => captured = e);

        _employeeAccountServiceMock
            .Setup(s => s.CreateAccountAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>()))
            .Returns(Task.CompletedTask);

        _employeeAccountServiceMock
            .Setup(s => s.GetRoleByEmployeeIdAsync(1))
            .ReturnsAsync((string?)null);

        await _service.CreateEmployeeAsync(dto);

        Assert.NotNull(captured);
        Assert.Null(captured!.MiddleName);
    }

    [Fact]
    public async Task UpdateEmployeeAsync_ShouldThrowKeyNotFoundException_WhenEmployeeDoesNotExist()
    {
        var dto = new UpdateEmployeeDto
        {
            FirstName = "Alice",
            LastName = "Smith",
            Email = "alice@sibers.com"
        };

        _employeeRepoMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Employee?)null);

        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.UpdateEmployeeAsync(999, dto));
        Assert.Contains("Employee with ID 999 was not found", exception.Message);
    }

    [Fact]
    public async Task DeleteEmployeeAsync_ShouldThrowKeyNotFoundException_WhenEmployeeDoesNotExist()
    {
        _employeeRepoMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Employee?)null);

        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.DeleteEmployeeAsync(999));
        Assert.Contains("Employee with ID 999 was not found", exception.Message);
    }

    [Fact]
    public async Task DeleteEmployeeAsync_ShouldDelete_WhenEmployeeExists()
    {
        var employee = new Employee { Id = 1, FirstName = "Alice", LastName = "Smith" };
        _employeeRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(employee);
        _employeeAccountServiceMock
            .Setup(s => s.DeleteAccountByEmployeeIdAsync(1))
            .Returns(Task.CompletedTask);

        await _service.DeleteEmployeeAsync(1);

        _employeeAccountServiceMock.Verify(s => s.DeleteAccountByEmployeeIdAsync(1), Times.Once);
        _employeeRepoMock.Verify(r => r.Delete(employee), Times.Once);
    }

    [Fact]
    public async Task SetUserIdAsync_ShouldUpdateUserId_WhenEmployeeExists()
    {
        var employee = new Employee { Id = 1, FirstName = "Alice", LastName = "Smith" };
        _employeeRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(employee);

        await _service.SetUserIdAsync(1, "identity-user-id");

        Assert.Equal("identity-user-id", employee.UserId);
        _employeeRepoMock.Verify(r => r.Update(employee), Times.Once);
    }
}
