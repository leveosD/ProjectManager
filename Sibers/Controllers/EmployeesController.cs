using Microsoft.AspNetCore.Authorization;
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

    public EmployeesController(IEmployeeService employeeService)
    {
        _employeeService = employeeService;
    }

    [HttpGet]
    public async Task<ActionResult<List<EmployeeDto>>> GetAll()
    {
        var employees = await _employeeService.GetAllEmployeesAsync();
        return Ok(employees);
    }

    [HttpGet("search")]
    public async Task<ActionResult<List<EmployeeDto>>> Search([FromQuery] string? q, [FromQuery] string? roles)
    {
        var employees = await _employeeService.SearchEmployeesAsync(q, roles);
        return Ok(employees);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<EmployeeDto>> GetById(int id)
    {
        var employee = await _employeeService.GetEmployeeByIdAsync(id);
        if (employee == null)
        {
            return NotFound(new { message = $"Employee with ID {id} was not found." });
        }

        return Ok(employee);
    }

    [HttpPost]
    [Authorize(Roles = UserRoles.Director)]
    public async Task<ActionResult<EmployeeDto>> Create([FromBody] CreateEmployeeDto dto)
    {
        try
        {
            var result = await _employeeService.CreateEmployeeAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
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
            await _employeeService.DeleteEmployeeAsync(id);
            return NoContent();
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
}
