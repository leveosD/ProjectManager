using Sibers.Core.Entities;

namespace Sibers.Core.Interfaces;

public interface IEmployeeRepository
{
    Task<Employee?> GetByIdAsync(int id);
    Task<Employee?> GetByUserIdAsync(string userId);
    Task<List<Employee>> GetAllAsync();
    Task<List<Employee>> SearchEmployeesAsync(string? query);
    Task<Employee> AddAsync(Employee employee);
    Task UpdateAsync(Employee employee);
    Task DeleteAsync(Employee employee);
}
