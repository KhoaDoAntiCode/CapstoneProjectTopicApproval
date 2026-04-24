using CapstoneRegistration.API.Data;
using CapstoneRegistration.API.Models;
using CapstoneRegistration.API.Repositories.Interfaces;

namespace CapstoneRegistration.API.UnitOfWorks;

public class UnitOfWork : IUnitOfWork
{
    public UnitOfWork(
        ApplicationDbContext context,
        IBaseRepository<User> users,
        ICapstoneProjectRepository capstoneProjects,
        IProjectReviewRepository projectReviews)
    {
        Context = context;
        Users = users;
        CapstoneProjects = capstoneProjects;
        ProjectReviews = projectReviews;
    }

    public ApplicationDbContext Context { get; }
    public IBaseRepository<User> Users { get; }
    public ICapstoneProjectRepository CapstoneProjects { get; }
    public IProjectReviewRepository ProjectReviews { get; }

    public Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        Context.SaveChangesAsync(ct);
}
