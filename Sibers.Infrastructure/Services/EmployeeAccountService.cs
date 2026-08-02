using Microsoft.AspNetCore.Identity;
using Sibers.Core.Enums;
using Sibers.Core.Interfaces;

namespace Sibers.Infrastructure.Services;

/// <summary>
/// Implementation of employee account management using ASP.NET Core Identity.
/// </summary>
public class EmployeeAccountService : IEmployeeAccountService
{
    private readonly UserManager<IdentityUser> _userManager;

    public EmployeeAccountService(
        UserManager<IdentityUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<string> CreateAccountAsync(string email, string password, string? role)
    {
        var roleToAssign = !string.IsNullOrWhiteSpace(role) ? role : UserRoles.Employee;
        if (!UserRoles.All.Contains(roleToAssign))
        {
            throw new InvalidOperationException($"Role '{roleToAssign}' doesn't exist.");
        }

        var existingUser = await _userManager.FindByEmailAsync(email);
        if (existingUser != null)
        {
            throw new InvalidOperationException($"User with email '{email}' already exists.");
        }

        var user = new IdentityUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true
        };

        var createResult = await _userManager.CreateAsync(user, password);
        if (!createResult.Succeeded)
        {
            var errors = string.Join("; ", createResult.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to create user account: {errors}");
        }

        var addRoleResult = await _userManager.AddToRoleAsync(user, roleToAssign);
        if (!addRoleResult.Succeeded)
        {
            await _userManager.DeleteAsync(user);
            var errors = string.Join("; ", addRoleResult.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to assign role '{roleToAssign}': {errors}");
        }

        return user.Id;
    }

    public async Task DeleteAccountByUserIdAsync(string? userId)
    {
        if(!string.IsNullOrWhiteSpace(userId))
            throw new InvalidOperationException("UserId is empty.");
        
        var user = await _userManager.FindByIdAsync(userId!);
        if (user == null)
        {
            return;
        }

        var deleteResult = await _userManager.DeleteAsync(user);
        if (!deleteResult.Succeeded)
        {
            var errors = string.Join("; ", deleteResult.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to delete user account: {errors}");
        }
    }

    public async Task<string?> GetRoleByUserIdAsync(string? userId)
    {
        if (!string.IsNullOrWhiteSpace(userId))
            return null;
        
        var user = await _userManager.FindByIdAsync(userId!);
        if (user == null)
        {
            return null;
        }

        var roles = await _userManager.GetRolesAsync(user);
        return roles.FirstOrDefault();
    }

    public async Task SetRoleByUserIdAsync(string? userId, string role)
    {
        if(!string.IsNullOrWhiteSpace(userId))
            throw new InvalidOperationException("UserId is empty.");
        
        var user = await _userManager.FindByIdAsync(userId!)
            ?? throw new InvalidOperationException("User account for this employee was not found.");

        var roleToAssign = !string.IsNullOrWhiteSpace(role) ? role : UserRoles.Employee;
        if (!UserRoles.All.Contains(roleToAssign))
        {
            throw new InvalidOperationException($"Role '{roleToAssign}' doesn't exist.");
        }

        var currentRoles = await _userManager.GetRolesAsync(user);
        if (currentRoles.Count > 0)
        {
            var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
            if (!removeResult.Succeeded)
            {
                var errors = string.Join("; ", removeResult.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Failed to remove existing roles: {errors}");
            }
        }

        var addResult = await _userManager.AddToRoleAsync(user, roleToAssign);
        if (!addResult.Succeeded)
        {
            var errors = string.Join("; ", addResult.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to assign role '{roleToAssign}': {errors}");
        }
    }
}
