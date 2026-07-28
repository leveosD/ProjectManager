using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Moq;
using Sibers.Core.DTOs;
using Sibers.Core.Entities;
using Sibers.Core.Interfaces;
using Sibers.Infrastructure.Services;

namespace Sibers.Tests;

public class AuthServiceTests
{
    private readonly Mock<UserManager<IdentityUser>> _userManagerMock;
    private readonly Mock<IEmployeeService> _employeeServiceMock = new();
    private readonly Mock<IConfiguration> _configurationMock = new();
    private readonly AuthService _service;

    public AuthServiceTests()
    {
        var store = new Mock<IUserStore<IdentityUser>>();
        _userManagerMock = new Mock<UserManager<IdentityUser>>(store.Object, null!, null!, null!, null!, null!, null!, null!, null!);

        _configurationMock.Setup(c => c.GetSection("Jwt")["Key"]).Returns("SuperSecretKeyForSibersProjectManagementSystem2026!");
        _configurationMock.Setup(c => c.GetSection("Jwt")["Issuer"]).Returns("SibersAPI");
        _configurationMock.Setup(c => c.GetSection("Jwt")["Audience"]).Returns("SibersClient");
        _configurationMock.Setup(c => c.GetSection("Jwt")["ExpiryInMinutes"]).Returns("60");

        _service = new AuthService(_userManagerMock.Object, _employeeServiceMock.Object, _configurationMock.Object);
    }

    [Fact]
    public async Task LoginAsync_ShouldThrowUnauthorizedAccessException_WhenUserNotFound()
    {
        var dto = new LoginDto { Email = "missing@sibers.com", Password = "Password123!" };
        _userManagerMock.Setup(um => um.FindByEmailAsync(dto.Email)).ReturnsAsync((IdentityUser?)null);

        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.LoginAsync(dto));
        Assert.Contains("Invalid email or password", exception.Message);
    }

    [Fact]
    public async Task LoginAsync_ShouldThrowUnauthorizedAccessException_WhenPasswordInvalid()
    {
        var dto = new LoginDto { Email = "user@sibers.com", Password = "WrongPassword" };
        var user = new IdentityUser { Id = "user-id", Email = dto.Email };

        _userManagerMock.Setup(um => um.FindByEmailAsync(dto.Email)).ReturnsAsync(user);
        _userManagerMock.Setup(um => um.CheckPasswordAsync(user, dto.Password)).ReturnsAsync(false);

        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.LoginAsync(dto));
        Assert.Contains("Invalid email or password", exception.Message);
    }

    [Fact]
    public async Task LoginAsync_ShouldThrowUnauthorizedAccessException_WhenEmployeeNotFound()
    {
        var dto = new LoginDto { Email = "user@sibers.com", Password = "Password123!" };
        var user = new IdentityUser { Id = "user-id", Email = dto.Email };

        _userManagerMock.Setup(um => um.FindByEmailAsync(dto.Email)).ReturnsAsync(user);
        _userManagerMock.Setup(um => um.CheckPasswordAsync(user, dto.Password)).ReturnsAsync(true);
        _employeeServiceMock.Setup(es => es.GetEmployeeByUserIdAsync(user.Id)).ReturnsAsync((Employee?)null);

        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.LoginAsync(dto));
        Assert.Contains("User account is not associated with an active employee", exception.Message);
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnTokenAndUserInfo_WhenCredentialsValid()
    {
        var dto = new LoginDto { Email = "director@sibers.com", Password = "Password123!" };
        var user = new IdentityUser { Id = "user-id", Email = dto.Email };
        var employee = new Employee { Id = 1, FirstName = "Alice", LastName = "Director", Email = dto.Email };

        _userManagerMock.Setup(um => um.FindByEmailAsync(dto.Email)).ReturnsAsync(user);
        _userManagerMock.Setup(um => um.CheckPasswordAsync(user, dto.Password)).ReturnsAsync(true);
        _employeeServiceMock.Setup(es => es.GetEmployeeByUserIdAsync(user.Id)).ReturnsAsync(employee);
        _userManagerMock.Setup(um => um.GetRolesAsync(user)).ReturnsAsync(["Director"]);

        var result = await _service.LoginAsync(dto);

        Assert.NotNull(result);
        Assert.False(string.IsNullOrWhiteSpace(result.Token));
        Assert.Equal(dto.Email, result.Email);
        Assert.Equal("Director", result.Role);
        Assert.Equal(1, result.EmployeeId);
        Assert.Equal("Director Alice", result.FullName);
        Assert.True(result.ExpiresAt > DateTime.UtcNow);
    }

    [Fact]
    public async Task LoginAsync_ShouldDefaultRoleToEmployee_WhenNoRolesAssigned()
    {
        var dto = new LoginDto { Email = "user@sibers.com", Password = "Password123!" };
        var user = new IdentityUser { Id = "user-id", Email = dto.Email };
        var employee = new Employee { Id = 2, FirstName = "Bob", LastName = "User", Email = dto.Email };

        _userManagerMock.Setup(um => um.FindByEmailAsync(dto.Email)).ReturnsAsync(user);
        _userManagerMock.Setup(um => um.CheckPasswordAsync(user, dto.Password)).ReturnsAsync(true);
        _employeeServiceMock.Setup(es => es.GetEmployeeByUserIdAsync(user.Id)).ReturnsAsync(employee);
        _userManagerMock.Setup(um => um.GetRolesAsync(user)).ReturnsAsync(new List<string>());

        var result = await _service.LoginAsync(dto);

        Assert.Equal("Employee", result.Role);
    }
}
