using Sibers.Core.DTOs;
using Sibers.Core.Entities;

namespace Sibers.Core.Interfaces;

public interface IEmployeeService
{
    Task<List<EmployeeDto>> GetAllEmployeesAsync();
    Task<List<EmployeeDto>> SearchEmployeesAsync(string? query);
    Task<EmployeeDto?> GetEmployeeByIdAsync(int id);
    Task<Employee?> GetEmployeeByUserIdAsync(string userId);
    Task<Employee> CreateEmployeeAsync(CreateEmployeeDto dto);
    Task<EmployeeDto> UpdateEmployeeAsync(int id, UpdateEmployeeDto dto);
    Task DeleteEmployeeAsync(int id);
    Task SetUserIdAsync(int employeeId, string userId);
}
