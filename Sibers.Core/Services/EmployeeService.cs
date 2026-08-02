using Sibers.Core.DTOs;
using Sibers.Core.Entities;
using Sibers.Core.Interfaces;

namespace Sibers.Core.Services;

public class EmployeeService : IEmployeeService
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IEmployeeAccountService _employeeAccountService;
    private readonly IUnitOfWork _unitOfWork;

    public EmployeeService(
        IEmployeeRepository employeeRepository,
        IEmployeeAccountService employeeAccountService,
        IUnitOfWork unitOfWork)
    {
        _employeeRepository = employeeRepository;
        _employeeAccountService = employeeAccountService;
        _unitOfWork = unitOfWork;
    }

    public async Task<List<EmployeeDto>> GetAllEmployeesAsync()
    {
        var employees = await _employeeRepository.GetAllAsync();
        return await EnrichWithRolesAsync(employees);
    }

    public async Task<List<EmployeeDto>> SearchEmployeesAsync(string? query, string? roles = null)
    {
        var roleList = ParseRoles(roles);
        var employees = await _employeeRepository.SearchEmployeesAsync(query, roleList);
        return await EnrichWithRolesAsync(employees);
    }

    public async Task<EmployeeDto?> GetEmployeeByIdAsync(int id)
    {
        var employee = await _employeeRepository.GetByIdAsync(id);
        if (employee == null)
        {
            return null;
        }

        var dto = MapToDto(employee);
        dto.Role = await _employeeAccountService.GetRoleByUserIdAsync(employee.UserId);
        return dto;
    }

    public async Task<Employee?> GetEmployeeByUserIdAsync(string userId)
    {
        return await _employeeRepository.GetByUserIdAsync(userId);
    }

    public async Task<EmployeeDto> CreateEmployeeAsync(CreateEmployeeDto dto)
    {
        return await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            var employee = new Employee
            {
                FirstName = dto.FirstName.Trim(),
                LastName = dto.LastName.Trim(),
                MiddleName = string.IsNullOrWhiteSpace(dto.MiddleName) ? null : dto.MiddleName.Trim(),
                Email = dto.Email.Trim().ToLower()
            };

            _employeeRepository.Add(employee);
            await _unitOfWork.CommitTransactionAsync();

            await _employeeAccountService.CreateAccountAsync(
                employee.Email,
                dto.Password,
                dto.Role);

            var result = await GetEmployeeByIdAsync(employee.Id);
            return result!;
        });
    }

    public async Task<EmployeeDto> UpdateEmployeeAsync(int id, UpdateEmployeeDto dto)
    {
        return await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            var employee = await _employeeRepository.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"Employee with ID {id} was not found.");

            employee.FirstName = dto.FirstName.Trim();
            employee.LastName = dto.LastName.Trim();
            employee.MiddleName = string.IsNullOrWhiteSpace(dto.MiddleName) ? null : dto.MiddleName.Trim();
            employee.Email = dto.Email.Trim().ToLower();

            _employeeRepository.Update(employee);

            if (!string.IsNullOrWhiteSpace(dto.Role))
            {
                await _employeeAccountService.SetRoleByUserIdAsync(employee.UserId, dto.Role);
            }

            var result = await GetEmployeeByIdAsync(id);
            return result!;
        });
    }

    public async Task DeleteEmployeeAsync(int id)
    {
        await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            var employee = await _employeeRepository.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"Employee with ID {id} was not found.");

            await _employeeAccountService.DeleteAccountByUserIdAsync(employee.UserId);
            _employeeRepository.Delete(employee);
        });
    }

    public async Task SetUserIdAsync(int employeeId, string userId)
    {
        await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            var employee = await _employeeRepository.GetByIdAsync(employeeId)
                ?? throw new KeyNotFoundException($"Employee with ID {employeeId} was not found.");

            employee.UserId = userId;
            _employeeRepository.Update(employee);
        });
    }

    private static IEnumerable<string>? ParseRoles(string? roles)
    {
        if (string.IsNullOrWhiteSpace(roles))
        {
            return null;
        }

        return roles
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(r => !string.IsNullOrWhiteSpace(r))
            .ToList();
    }

    private async Task<List<EmployeeDto>> EnrichWithRolesAsync(List<Employee> employees)
    {
        var dtos = new List<EmployeeDto>(employees.Count);
        foreach (var employee in employees)
        {
            var dto = MapToDto(employee);
            dto.Role = await _employeeAccountService.GetRoleByUserIdAsync(employee.UserId);
            dtos.Add(dto);
        }
        return dtos;
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
