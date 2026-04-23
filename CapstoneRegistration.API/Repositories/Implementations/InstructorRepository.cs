using Microsoft.EntityFrameworkCore;
using CapstoneRegistration.API.Data;
using CapstoneRegistration.API.Models;
using CapstoneRegistration.API.Repositories.Interfaces;

namespace CapstoneRegistration.API.Repositories.Implementations;

public class InstructorRepository : BaseRepository<Instructor>, IInstructorRepository
{
    public InstructorRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Instructor?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        return await _context.Instructors
            .FirstOrDefaultAsync(i => i.Email == email, ct);
    }

    public async Task<bool> EmailExistsAsync(string email, Guid? excludeId = null, CancellationToken ct = default)
    {
        var query = _context.Instructors.Where(i => i.Email == email);
        
        if (excludeId.HasValue)
        {
            query = query.Where(i => i.Id != excludeId.Value);
        }

        return await query.AnyAsync(ct);
    }
}
