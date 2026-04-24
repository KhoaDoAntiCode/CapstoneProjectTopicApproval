using CapstoneRegistration.API.DTOs.Requests;
using CapstoneRegistration.API.DTOs.Responses;
using CapstoneRegistration.API.Exceptions;
using CapstoneRegistration.API.Services.Interfaces;

namespace CapstoneRegistration.API.Services.Implementations;

public class ReviewService : IReviewService
{
    public async Task<ReviewResponse> SubmitReviewAsync(
        Guid projectId, Guid reviewerId, ReviewRequest request, CancellationToken ct = default)
    {
        await Task.CompletedTask;
        throw new BadRequestException("Review workflow is not configured in the current SQL Server schema.");
    }

    public async Task<IReadOnlyList<ReviewResponse>> GetReviewsByProjectAsync(
        Guid projectId, CancellationToken ct = default)
    {
        await Task.CompletedTask;
        return [];
    }
}
