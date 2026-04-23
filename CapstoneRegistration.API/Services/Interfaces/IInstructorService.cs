using CapstoneRegistration.API.DTOs.Requests;
using CapstoneRegistration.API.DTOs.Responses;
using CapstoneRegistration.API.Common;

namespace CapstoneRegistration.API.Services.Interfaces;

public interface IInstructorService
{
    Task<InstructorResponse> CreateAsync(CreateInstructorRequest request, CancellationToken ct = default);
    Task<InstructorResponse> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<PagedResult<InstructorResponse>> GetPagedAsync(int page, int pageSize, string? search = null, CancellationToken ct = default);
    Task<InstructorResponse> UpdateAsync(Guid id, UpdateInstructorRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
