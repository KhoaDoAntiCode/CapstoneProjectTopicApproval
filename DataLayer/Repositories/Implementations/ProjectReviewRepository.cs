using Microsoft.EntityFrameworkCore;
using CapstoneRegistration.API.Data;
using CapstoneRegistration.API.Models;
using CapstoneRegistration.API.Repositories.Interfaces;

namespace CapstoneRegistration.API.Repositories.Implementations;

public class ProjectReviewRepository : IProjectReviewRepository
{
    private readonly ApplicationDbContext _db;
    public ProjectReviewRepository(ApplicationDbContext db) => _db = db;

    public async Task<IReadOnlyList<ProjectReview>> GetByProjectIdAsync(
        Guid projectId, CancellationToken ct = default) =>
        await _db.ProjectReviews
            .Include(r => r.ReviewedBy)
            .Where(r => r.ProjectId == projectId)
            .OrderByDescending(r => r.ReviewedAt)
            .ToListAsync(ct);

    public Task<ProjectReview> AddAsync(ProjectReview review, CancellationToken ct = default)
    {
        _db.ProjectReviews.Add(review);
        return Task.FromResult(review);
    }
}
