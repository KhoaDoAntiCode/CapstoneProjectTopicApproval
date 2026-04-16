using CapstoneRegistration.API.Common;
using CapstoneRegistration.API.Models;

namespace CapstoneRegistration.API.Repositories.Interfaces;

public interface ICapstoneProjectRepository
{
    Task<CapstoneProject?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default);
    Task<PagedResult<CapstoneProject>> GetPagedAsync(
        int page, int pageSize,
        string? semesterId = null,
        string? status = null,
        string? search = null,
        CancellationToken ct = default);
    Task<string> GenerateProjectCodeAsync(string semesterId, CancellationToken ct = default);
    Task<CapstoneProject> AddAsync(CapstoneProject project, CancellationToken ct = default);
}
