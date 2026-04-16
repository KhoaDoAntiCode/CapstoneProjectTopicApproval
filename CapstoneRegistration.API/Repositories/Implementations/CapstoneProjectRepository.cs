using Microsoft.EntityFrameworkCore;
using CapstoneRegistration.API.Common;
using CapstoneRegistration.API.Data;
using CapstoneRegistration.API.Models;
using CapstoneRegistration.API.Repositories.Interfaces;

namespace CapstoneRegistration.API.Repositories.Implementations;

public class CapstoneProjectRepository : ICapstoneProjectRepository
{
    private readonly ApplicationDbContext _db;
    public CapstoneProjectRepository(ApplicationDbContext db) => _db = db;

    public async Task<CapstoneProject?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default) =>
        await _db.CapstoneProjects
            .Include(p => p.Supervisors.OrderBy(s => s.DisplayOrder))
            .Include(p => p.Students.OrderBy(s => s.DisplayOrder))
            .Include(p => p.ProjectReviews.OrderBy(r => r.ReviewedAt))
                .ThenInclude(r => r.ReviewedBy)
            .Include(p => p.CreatedBy)
            .FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<PagedResult<CapstoneProject>> GetPagedAsync(
        int page, int pageSize,
        string? semesterId = null,
        string? status = null,
        string? search = null,
        CancellationToken ct = default)
    {
        var query = _db.CapstoneProjects
            .Include(p => p.CreatedBy)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(semesterId))
            query = query.Where(p => p.SemesterId == semesterId);

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(p => p.Status == status);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(p =>
                p.EnglishName.Contains(search) ||
                p.VietnameseName.Contains(search) ||
                p.ProjectCode.Contains(search));

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return PagedResult<CapstoneProject>.Create(items, total, page, pageSize);
    }

    public async Task<string> GenerateProjectCodeAsync(string semesterId, CancellationToken ct = default)
    {
        var count = await _db.CapstoneProjects
            .CountAsync(p => p.SemesterId == semesterId, ct);
        return $"{semesterId}{(count + 1):D3}";
    }

    public async Task<CapstoneProject> AddAsync(CapstoneProject project, CancellationToken ct = default)
    {
        _db.CapstoneProjects.Add(project);
        await _db.SaveChangesAsync(ct);
        return project;
    }
}
