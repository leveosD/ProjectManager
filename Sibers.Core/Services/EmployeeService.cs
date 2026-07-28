using Sibers.Core.DTOs;
using Sibers.Core.Entities;
using Sibers.Core.Interfaces;

namespace Sibers.Core.Services;

public class EmployeeService : IEmployeeService
{
    private readonly IEmployeeRepository _employeeRepository;

    public EmployeeService(IEmployeeRepository employeeRepository)
    {
        _employeeRepository = employeeRepository;
    }

    public async Task<List<EmployeeDto>> GetAllEmployeesAsync()
    {
        var employees = await _employeeRepository.GetAllAsync();
        return employees.Select(MapToDto).ToList();
    }

    public async Task<List<EmployeeDto>> SearchEmployeesAsync(string? query)
    {
        var employees = await _employeeRepository.SearchEmployeesAsync(query);
        return employees.Select(MapToDto).ToList();
    }

    public async Task<EmployeeDto?> GetEmployeeByIdAsync(int id)
    {
        var employee = await _employeeRepository.GetByIdAsync(id);
        return employee == null ? null : MapToDto(employee);
    }

    public async Task<Employee?> GetEmployeeByUserIdAsync(string userId)
    {
        return await _employeeRepository.GetByUserIdAsync(userId);
    }

    public async Task<Employee> CreateEmployeeAsync(CreateEmployeeDto dto)
    {
        var employee = new Employee
        {
            FirstName = dto.FirstName.Trim(),
            LastName = dto.LastName.Trim(),
            MiddleName = string.IsNullOrWhiteSpace(dto.MiddleName) ? null : dto.MiddleName.Trim(),
            Email = dto.Email.Trim().ToLower()
        };

        return await _employeeRepository.AddAsync(employee);
    }

    public async Task<EmployeeDto> UpdateEmployeeAsync(int id, UpdateEmployeeDto dto)
    {
        var employee = await _employeeRepository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Employee with ID {id} was not found.");

        employee.FirstName = dto.FirstName.Trim();
        employee.LastName = dto.LastName.Trim();
        employee.MiddleName = string.IsNullOrWhiteSpace(dto.MiddleName) ? null : dto.MiddleName.Trim();
        employee.Email = dto.Email.Trim().ToLower();

        await _employeeRepository.UpdateAsync(employee);
        return MapToDto(employee);
    }

    public async Task DeleteEmployeeAsync(int id)
    {
        var employee = await _employeeRepository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Employee with ID {id} was not found.");

        await _employeeRepository.DeleteAsync(employee);
    }

    public async Task SetUserIdAsync(int employeeId, string userId)
    {
        var employee = await _employeeRepository.GetByIdAsync(employeeId)
            ?? throw new KeyNotFoundException($"Employee with ID {employeeId} was not found.");

        employee.UserId = userId;
        await _employeeRepository.UpdateAsync(employee);
    }

    private static EmployeeDto MapToDto(Employee e) => new()
    {
        Id = e.Id,
        FirstName = e.FirstName,
        LastName = e.LastName,
        MiddleName = e.MiddleName,
        Email = e.Email,
        FullName = e.FullName,
        UserId = e.UserId
    };
}
