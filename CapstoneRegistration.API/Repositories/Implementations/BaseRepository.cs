using Microsoft.EntityFrameworkCore;
using CapstoneRegistration.API.Data;
using CapstoneRegistration.API.Repositories.Interfaces;

namespace CapstoneRegistration.API.Repositories.Implementations;

public class BaseRepository<T, TKey> : IBaseRepository<T, TKey> where T : class
{
    protected readonly ApplicationDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public BaseRepository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public virtual async Task<T?> GetByIdAsync(TKey id, CancellationToken ct = default) =>
        await _dbSet.FindAsync([id], ct);

    public virtual async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default) =>
        await _dbSet.ToListAsync(ct);

    public virtual async Task<T> AddAsync(T entity, CancellationToken ct = default)
    {
        await _dbSet.AddAsync(entity, ct);
        await _context.SaveChangesAsync(ct);
        return entity;
    }

    public virtual async Task UpdateAsync(T entity, CancellationToken ct = default)
    {
        _dbSet.Update(entity);
        await _context.SaveChangesAsync(ct);
    }

    public virtual async Task DeleteAsync(T entity, CancellationToken ct = default)
    {
        _dbSet.Remove(entity);
        await _context.SaveChangesAsync(ct);
    }

    public virtual async Task<bool> ExistsAsync(TKey id, CancellationToken ct = default) =>
        await GetByIdAsync(id, ct) is not null;
}

public class BaseRepository<T> : BaseRepository<T, Guid>, IBaseRepository<T> where T : class
{
    public BaseRepository(ApplicationDbContext context) : base(context) { }
}
