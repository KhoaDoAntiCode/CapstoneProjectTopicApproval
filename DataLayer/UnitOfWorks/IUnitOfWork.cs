using CapstoneRegistration.API.Data;
using CapstoneRegistration.API.Models;
using CapstoneRegistration.API.Repositories.Interfaces;

namespace CapstoneRegistration.API.UnitOfWorks;

public interface IUnitOfWork
{
    ApplicationDbContext Context { get; }
    IBaseRepository<User> Users { get; }
    ICapstoneProjectRepository CapstoneProjects { get; }
    IProjectReviewRepository ProjectReviews { get; }
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
