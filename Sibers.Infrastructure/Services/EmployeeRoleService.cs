using Microsoft.AspNetCore.Identity;
using Sibers.Core.Interfaces;

namespace Sibers.Infrastructure.Services;

public class EmployeeRoleService : IEmployeeRoleService
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IEmployeeRepository _employeeRepository;

    public EmployeeRoleService(
        UserManager<IdentityUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IEmployeeRepository employeeRepository)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _employeeRepository = employeeRepository;
    }

    public async Task<string?> GetRoleByEmployeeIdAsync(int employeeId)
    {
        var employee = await _employeeRepository.GetByIdAsync(employeeId);
        if (employee?.UserId == null)
        {
            return null;
        }

        var user = await _userManager.FindByIdAsync(employee.UserId);
        if (user == null)
        {
            return null;
        }

        var roles = await _userManager.GetRolesAsync(user);
        return roles.FirstOrDefault();
    }

    public async Task SetRoleAsync(int employeeId, string role)
    {
        var employee = await _employeeRepository.GetByIdAsync(employeeId)
            ?? throw new KeyNotFoundException($"Employee with ID {employeeId} was not found.");

        if (employee.UserId == null)
        {
            throw new InvalidOperationException("Cannot change role for an employee without a user account.");
        }

        var user = await _userManager.FindByIdAsync(employee.UserId)
            ?? throw new InvalidOperationException("User account for this employee was not found.");

        var currentRoles = await _userManager.GetRolesAsync(user);
        if (currentRoles.Count > 0)
        {
            var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
            if (!removeResult.Succeeded)
            {
                throw new InvalidOperationException("Failed to remove existing roles.");
            }
        }

        if (!await _roleManager.RoleExistsAsync(role))
        {
            await _roleManager.CreateAsync(new IdentityRole(role));
        }

        var addResult = await _userManager.AddToRoleAsync(user, role);
        if (!addResult.Succeeded)
        {
            throw new InvalidOperationException($"Failed to assign role '{role}'.");
        }
    }
}
