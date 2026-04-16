using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CapstoneRegistration.API.Common;
using CapstoneRegistration.API.DTOs.Requests;
using CapstoneRegistration.API.Exceptions;
using CapstoneRegistration.API.Services.Interfaces;

namespace CapstoneRegistration.API.Controllers;

[ApiController]
[Route("api/projects")]
[Authorize]
public class ProjectController : ControllerBase
{
    private readonly IProjectService _projectService;

    public ProjectController(IProjectService projectService) =>
        _projectService = projectService;

    [HttpPost("parse")]
    public async Task<IActionResult> ParseDocx(
        IFormFile file,
        CancellationToken ct)
    {
        if (file is null || file.Length == 0)
            throw new BadRequestException("No file uploaded.");

        if (!file.FileName.EndsWith(".docx", StringComparison.OrdinalIgnoreCase))
            throw new BadRequestException("Only .docx files are accepted.");

        await using var stream = file.OpenReadStream();
        var preview = await _projectService.ParseDocxAsync(stream, ct);
        return Ok(ApiResponse<object>.Ok(preview));
    }

    [HttpPost]
    public async Task<IActionResult> Submit(
        [FromBody] SubmitProjectRequest request,
        CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        var result = await _projectService.SubmitAsync(userId, request, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id },
            ApiResponse<object>.Ok(result, "Project submitted successfully."));
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? semesterId = null,
        [FromQuery] string? status = null,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
    {
        if (page < 1) page = 1;
        if (pageSize is < 1 or > 100) pageSize = 20;

        var result = await _projectService.GetPagedAsync(page, pageSize, semesterId, status, search, ct);
        return Ok(ApiResponse<object>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _projectService.GetByIdAsync(id, ct);
        return Ok(ApiResponse<object>.Ok(result));
    }

    private Guid GetCurrentUserId()
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier)
               ?? User.FindFirstValue("sub");
        if (sub is null || !Guid.TryParse(sub, out var userId))
            throw new UnauthorizedException("Cannot determine current user.");
        return userId;
    }
}
