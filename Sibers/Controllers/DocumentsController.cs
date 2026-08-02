using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sibers.Core.DTOs;
using Sibers.Core.Enums;
using Sibers.Core.Interfaces;

namespace Sibers.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DocumentsController : ControllerBase
{
    private readonly IDocumentService _documentService;

    public DocumentsController(IDocumentService documentService)
    {
        _documentService = documentService;
    }

    [HttpGet("project/{projectId}")]
    public async Task<ActionResult<List<ProjectDocumentDto>>> GetProjectDocuments(int projectId)
    {
        var docs = await _documentService.GetProjectDocumentsAsync(projectId);
        return Ok(docs);
    }

    [HttpPost("upload/{projectId}")]
    [Authorize(Roles = $"{UserRoles.Director},{UserRoles.ProjectManager}")]
    public async Task<ActionResult<ProjectDocumentDto>> Upload(int projectId, IFormFile file)
    {
        try
        {
            using var stream = file.OpenReadStream();
            var uploaded = await _documentService.UploadDocumentAsync(
                projectId,
                stream,
                file.FileName,
                file.ContentType,
                file.Length);

            return Ok(uploaded);
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

    [HttpGet("download/{id}")]
    public async Task<IActionResult> Download(int id)
    {
        var result = await _documentService.DownloadDocumentAsync(id);
        if (!result.HasValue)
        {
            return NotFound(new { message = $"Document with ID {id} was not found or file is missing." });
        }

        return File(result.Value.stream, result.Value.contentType, result.Value.fileName);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = $"{UserRoles.Director},{UserRoles.ProjectManager}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _documentService.DeleteDocumentAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}
