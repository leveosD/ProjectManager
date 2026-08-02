using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sibers.Core.DTOs;
using Sibers.Core.Enums;
using Sibers.Core.Interfaces;

namespace Sibers.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;

    public TasksController(ITaskService taskService)
    {
        _taskService = taskService;
    }

    [HttpGet]
    public async Task<ActionResult<List<ProjectTaskDto>>> GetTasks([FromQuery] ProjectTaskFilterDto filter)
    {
        try
        {
            var tasks = await _taskService.GetTasksAsync(filter);
            return Ok(tasks);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProjectTaskDto>> GetById(int id)
    {
        var task = await _taskService.GetTaskByIdAsync(id);
        if (task == null)
        {
            return NotFound(new { message = $"Task with ID {id} was not found." });
        }
        return Ok(task);
    }

    [HttpPost]
    [Authorize(Roles = $"{UserRoles.Director},{UserRoles.ProjectManager}")]
    public async Task<ActionResult<ProjectTaskDto>> Create([FromBody] CreateProjectTaskDto dto)
    {
        try
        {
            var created = await _taskService.CreateTaskAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
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

    [HttpPut("{id}")]
    [Authorize(Roles = $"{UserRoles.Director},{UserRoles.ProjectManager}")]
    public async Task<ActionResult<ProjectTaskDto>> Update(int id, [FromBody] UpdateProjectTaskDto dto)
    {
        try
        {
            var updated = await _taskService.UpdateTaskAsync(id, dto);
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

    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] ProjectTaskStatus status)
    {
        try
        {
            await _taskService.UpdateTaskStatusAsync(id, status);
            return Ok(new { message = "Task status updated successfully." });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPatch("{id}/executor")]
    [Authorize(Roles = $"{UserRoles.Director},{UserRoles.ProjectManager}")]
    public async Task<IActionResult> AssignExecutor(int id, [FromBody] int? executorId)
    {
        try
        {
            await _taskService.AssignTaskExecutorAsync(id, executorId);
            return Ok(new { message = "Task executor assigned successfully." });
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
    [Authorize(Roles = $"{UserRoles.Director},{UserRoles.ProjectManager}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _taskService.DeleteTaskAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}
