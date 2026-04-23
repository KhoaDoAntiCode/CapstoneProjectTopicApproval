using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CapstoneRegistration.API.Common;
using CapstoneRegistration.API.DTOs.Requests;
using CapstoneRegistration.API.DTOs.Responses;
using CapstoneRegistration.API.Services.Interfaces;

namespace CapstoneRegistration.API.Controllers;

[ApiController]
[Route("api/instructors")]
[Authorize]
public class InstructorController : ControllerBase
{
    private readonly IInstructorService _instructorService;

    public InstructorController(IInstructorService instructorService)
    {
        _instructorService = instructorService;
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateInstructorRequest request,
        CancellationToken ct)
    {
        var result = await _instructorService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id },
            ApiResponse<object>.Ok(result, "Instructor created successfully."));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _instructorService.GetByIdAsync(id, ct);
        return Ok(ApiResponse<object>.Ok(result));
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
    {
        if (page < 1) page = 1;
        if (pageSize is < 1 or > 100) pageSize = 20;

        var result = await _instructorService.GetPagedAsync(page, pageSize, search, ct);
        return Ok(ApiResponse<object>.Ok(result));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateInstructorRequest request,
        CancellationToken ct)
    {
        var result = await _instructorService.UpdateAsync(id, request, ct);
        return Ok(ApiResponse<object>.Ok(result, "Instructor updated successfully."));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _instructorService.DeleteAsync(id, ct);
        return Ok(ApiResponse<object>.Ok(new object(), "Instructor deleted successfully."));
    }
}
