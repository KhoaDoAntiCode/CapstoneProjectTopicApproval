using CapstoneRegistration.API.Data;
using CapstoneRegistration.API.DTOs.Requests;
using CapstoneRegistration.API.DTOs.Responses;
using CapstoneRegistration.API.Exceptions;
using CapstoneRegistration.API.Models;
using CapstoneRegistration.API.Repositories.Interfaces;
using CapstoneRegistration.API.Services.Interfaces;

namespace CapstoneRegistration.API.Services.Implementations;

public class ReviewService : IReviewService
{
    private readonly IProjectReviewRepository _reviewRepo;
    private readonly ICapstoneProjectRepository _projectRepo;
    private readonly ApplicationDbContext _db;

    public ReviewService(
        IProjectReviewRepository reviewRepo,
        ICapstoneProjectRepository projectRepo,
        ApplicationDbContext db)
    {
        _reviewRepo  = reviewRepo;
        _projectRepo = projectRepo;
        _db          = db;
    }

    public async Task<ReviewResponse> SubmitReviewAsync(
        Guid projectId, Guid reviewerId, ReviewRequest request, CancellationToken ct = default)
    {
        if (request.Decision is not ("Accepted" or "Denied"))
            throw new BadRequestException("Decision must be 'Accepted' or 'Denied'.");

        var project = await _projectRepo.GetByIdWithDetailsAsync(projectId, ct)
            ?? throw new NotFoundException("CapstoneProject", projectId);

        if (project.Status != "Pending")
            throw new BadRequestException(
                $"Project is already '{project.Status}' and cannot be reviewed again.");

        var review = new ProjectReview
        {
            ProjectId    = projectId,
            ReviewedById = reviewerId,
            Decision     = request.Decision,
            Comment      = request.Comment,
            ReviewedAt   = DateTime.UtcNow
        };

        project.Status    = request.Decision;
        project.UpdatedAt = DateTime.UtcNow;

        _db.ProjectReviews.Add(review);
        await _db.SaveChangesAsync(ct);

        var reviewerName = project.ProjectReviews
            .FirstOrDefault(r => r.ReviewedById == reviewerId)
            ?.ReviewedBy.FullName ?? "Unknown";

        return new ReviewResponse
        {
            Id             = review.Id,
            ProjectId      = review.ProjectId,
            ReviewedById   = review.ReviewedById,
            ReviewedByName = reviewerName,
            Decision       = review.Decision,
            Comment        = review.Comment,
            ReviewedAt     = review.ReviewedAt
        };
    }

    public async Task<IReadOnlyList<ReviewResponse>> GetReviewsByProjectAsync(
        Guid projectId, CancellationToken ct = default)
    {
        var reviews = await _reviewRepo.GetByProjectIdAsync(projectId, ct);
        return reviews.Select(r => new ReviewResponse
        {
            Id             = r.Id,
            ProjectId      = r.ProjectId,
            ReviewedById   = r.ReviewedById,
            ReviewedByName = r.ReviewedBy.FullName,
            Decision       = r.Decision,
            Comment        = r.Comment,
            ReviewedAt     = r.ReviewedAt
        }).ToList();
    }
}
