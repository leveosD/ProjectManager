using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Sibers.Core.DTOs;
using Sibers.Core.Enums;
using Sibers.Core.Interfaces;

namespace Sibers.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EmployeesController : ControllerBase
{
    private readonly IEmployeeService _employeeService;
    private readonly IEmployeeRoleService _employeeRoleService;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public EmployeesController(
        IEmployeeService employeeService,
        IEmployeeRoleService employeeRoleService,
        UserManager<IdentityUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        _employeeService = employeeService;
        _employeeRoleService = employeeRoleService;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    [HttpGet]
    public async Task<ActionResult<List<EmployeeDto>>> GetAll([FromQuery] IEnumerable<string>? roles)
    {
        var employees = await _employeeService.GetAllEmployeesAsync();
        var enriched = await EnrichWithRolesAsync(employees);
        var roleSet = roles?.Where(r => !string.IsNullOrWhiteSpace(r)).ToHashSet() ?? [];
        if (roleSet.Count > 0)
        {
            enriched = enriched.Where(e => e.Role != null && roleSet.Contains(e.Role)).ToList();
        }
        return Ok(enriched);
    }

    [HttpGet("search")]
    public async Task<ActionResult<List<EmployeeDto>>> Search([FromQuery] string? q, [FromQuery] IEnumerable<string>? roles)
    {
        var employees = await _employeeService.SearchEmployeesAsync(q);
        var enriched = await EnrichWithRolesAsync(employees);
        var roleSet = roles?.Where(r => !string.IsNullOrWhiteSpace(r)).ToHashSet() ?? [];
        if (roleSet.Count > 0)
        {
            enriched = enriched.Where(e => e.Role != null && roleSet.Contains(e.Role)).ToList();
        }
        return Ok(enriched);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<EmployeeDto>> GetById(int id)
    {
        var employee = await _employeeService.GetEmployeeByIdAsync(id);
        if (employee == null)
        {
            return NotFound(new { message = $"Employee with ID {id} was not found." });
        }

        employee.Role = await _employeeRoleService.GetRoleByEmployeeIdAsync(id);
        return Ok(employee);
    }

    private async Task<List<EmployeeDto>> EnrichWithRolesAsync(List<EmployeeDto> employees)
    {
        foreach (var employee in employees)
        {
            employee.Role = await _employeeRoleService.GetRoleByEmployeeIdAsync(employee.Id);
        }
        return employees;
    }

    [HttpPost]
    [Authorize(Roles = UserRoles.Director)]
    public async Task<ActionResult<EmployeeDto>> Create([FromBody] CreateEmployeeDto dto)
    {
        try
        {
            var existingUser = await _userManager.FindByEmailAsync(dto.Email);
            if (existingUser != null)
            {
                return BadRequest(new { message = $"User with email '{dto.Email}' already exists." });
            }

            var employee = await _employeeService.CreateEmployeeAsync(dto);

            var user = new IdentityUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                EmailConfirmed = true
            };

            var createResult = await _userManager.CreateAsync(user, dto.Password);
            if (!createResult.Succeeded)
            {
                await _employeeService.DeleteEmployeeAsync(employee.Id);
                var errors = string.Join("; ", createResult.Errors.Select(e => e.Description));
                return BadRequest(new { message = $"Failed to create user account: {errors}" });
            }

            await _employeeService.SetUserIdAsync(employee.Id, user.Id);

            var role = !string.IsNullOrWhiteSpace(dto.Role) ? dto.Role : UserRoles.Employee;
            if (!await _roleManager.RoleExistsAsync(role))
            {
                await _roleManager.CreateAsync(new IdentityRole(role));
            }
            await _userManager.AddToRoleAsync(user, role);

            var result = await _employeeService.GetEmployeeByIdAsync(employee.Id);
            result!.Role = role;
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = UserRoles.Director)]
    public async Task<ActionResult<EmployeeDto>> Update(int id, [FromBody] UpdateEmployeeDto dto)
    {
        try
        {
            var updated = await _employeeService.UpdateEmployeeAsync(id, dto);

            if (!string.IsNullOrWhiteSpace(dto.Role))
            {
                await _employeeRoleService.SetRoleAsync(id, dto.Role);
                updated.Role = dto.Role;
            }
            else
            {
                updated.Role = await _employeeRoleService.GetRoleByEmployeeIdAsync(id);
            }

            return Ok(updated);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = UserRoles.Director)]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var employee = await _employeeService.GetEmployeeByIdAsync(id)
                ?? throw new KeyNotFoundException($"Employee with ID {id} was not found.");

            if (!string.IsNullOrWhiteSpace(employee.UserId))
            {
                var user = await _userManager.FindByIdAsync(employee.UserId);
                if (user != null)
                {
                    var deleteResult = await _userManager.DeleteAsync(user);
                    if (!deleteResult.Succeeded)
                    {
                        var errors = string.Join("; ", deleteResult.Errors.Select(e => e.Description));
                        return BadRequest(new { message = $"Failed to delete user account: {errors}" });
                    }
                }
            }

            await _employeeService.DeleteEmployeeAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}
