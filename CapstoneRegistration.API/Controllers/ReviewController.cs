using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CapstoneRegistration.API.Common;
using CapstoneRegistration.API.DTOs.Requests;
using CapstoneRegistration.API.Exceptions;
using CapstoneRegistration.API.Services.Interfaces;

namespace CapstoneRegistration.API.Controllers;

[ApiController]
[Route("api/projects/{projectId:guid}/reviews")]
[Authorize]
public class ReviewController : ControllerBase
{
    private readonly IReviewService _reviewService;
    public ReviewController(IReviewService reviewService) => _reviewService = reviewService;

    [HttpPost]
    public async Task<IActionResult> Submit(
        Guid projectId,
        [FromBody] ReviewRequest request,
        CancellationToken ct)
    {
        var reviewerId = GetCurrentUserId();
        var result = await _reviewService.SubmitReviewAsync(projectId, reviewerId, request, ct);
        return Ok(ApiResponse<object>.Ok(result,
            $"Project {result.Decision.ToLower()} successfully."));
    }

    [HttpGet]
    public async Task<IActionResult> GetHistory(Guid projectId, CancellationToken ct)
    {
        var result = await _reviewService.GetReviewsByProjectAsync(projectId, ct);
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
