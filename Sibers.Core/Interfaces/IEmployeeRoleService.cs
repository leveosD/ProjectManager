namespace Sibers.Core.Interfaces;

/// <summary>
/// Provides access to ASP.NET Core Identity roles for employees.
/// Implementation lives in Infrastructure to keep Core free of Identity dependencies.
/// </summary>
public interface IEmployeeRoleService
{
    Task<string?> GetRoleByEmployeeIdAsync(int employeeId);
    Task SetRoleAsync(int employeeId, string role);
}
