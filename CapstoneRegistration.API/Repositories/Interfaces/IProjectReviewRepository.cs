using CapstoneRegistration.API.Models;

namespace CapstoneRegistration.API.Repositories.Interfaces;

public interface IProjectReviewRepository
{
    Task<IReadOnlyList<ProjectReview>> GetByProjectIdAsync(Guid projectId, CancellationToken ct = default);
    Task<ProjectReview> AddAsync(ProjectReview review, CancellationToken ct = default);
}
