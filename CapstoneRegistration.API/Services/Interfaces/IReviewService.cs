using CapstoneRegistration.API.DTOs.Requests;
using CapstoneRegistration.API.DTOs.Responses;

namespace CapstoneRegistration.API.Services.Interfaces;

public interface IReviewService
{
    Task<ReviewResponse> SubmitReviewAsync(
        Guid projectId, Guid reviewerId, ReviewRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<ReviewResponse>> GetReviewsByProjectAsync(
        Guid projectId, CancellationToken ct = default);
}
