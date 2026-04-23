using CapstoneRegistration.API.Models;

namespace CapstoneRegistration.API.Repositories.Interfaces;

public interface IInstructorRepository : IBaseRepository<Instructor>
{
    Task<Instructor?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<bool> EmailExistsAsync(string email, Guid? excludeId = null, CancellationToken ct = default);
}
