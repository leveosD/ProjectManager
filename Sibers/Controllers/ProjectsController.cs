using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sibers.Core.DTOs;
using Sibers.Core.Enums;
using Sibers.Core.Interfaces;

namespace Sibers.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProjectsController : ControllerBase
{
    private readonly IProjectService _projectService;

    public ProjectsController(IProjectService projectService)
    {
        _projectService = projectService;
    }

    [HttpGet]
    public async Task<ActionResult<List<ProjectDto>>> GetProjects([FromQuery] ProjectFilterDto filter)
    {
        var employeeId = User.FindFirst("EmployeeId")?.Value;
        var projects = await _projectService.GetProjectsAsync(filter, employeeId);

        return Ok(projects);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProjectDto>> GetById(int id)
    {
        var project = await _projectService.GetProjectByIdAsync(id);
        if (project == null)
        {
            return NotFound(new { message = $"Project with ID {id} was not found." });
        }
        return Ok(project);
    }

    [HttpPost]
    [Authorize(Roles = $"{UserRoles.Director}")]
    public async Task<ActionResult<ProjectDto>> Create([FromBody] CreateProjectDto dto)
    {
        try
        {
            var created = await _projectService.CreateProjectAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = $"{UserRoles.Director},{UserRoles.ProjectManager}")]
    public async Task<ActionResult<ProjectDto>> Update(int id, [FromBody] UpdateProjectDto dto)
    {
        try
        {
            var updated = await _projectService.UpdateProjectAsync(id, dto);
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
            await _projectService.DeleteProjectAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPost("{id}/employees")]
    [Authorize(Roles = $"{UserRoles.Director},{UserRoles.ProjectManager}")]
    public async Task<IActionResult> AddEmployees(int id, [FromBody] List<int> employeeIds)
    {
        try
        {
            await _projectService.AddEmployeesToProjectAsync(id, employeeIds);
            return Ok(new { message = "Employees added to project successfully." });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}/employees/{employeeId}")]
    [Authorize(Roles = $"{UserRoles.Director},{UserRoles.ProjectManager}")]
    public async Task<IActionResult> RemoveEmployee(int id, int employeeId)
    {
        try
        {
            await _projectService.RemoveEmployeeFromProjectAsync(id, employeeId);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}
