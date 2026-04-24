using CapstoneRegistration.API.Common;
using CapstoneRegistration.API.DTOs.Requests;
using CapstoneRegistration.API.DTOs.Responses;

namespace CapstoneRegistration.API.Services.Interfaces;

public interface IProjectService
{
    Task<DocxPreviewResponse> ParseDocxAsync(Stream docxStream, CancellationToken ct = default);
    Task<ProjectResponse> SubmitAsync(Guid createdById, SubmitProjectRequest request, CancellationToken ct = default);
    Task<ProjectResponse> UpdateAsync(Guid id, SubmitProjectRequest request, CancellationToken ct = default);
    Task<GeneratedFile> CreateWithDocxAsync(Guid createdById, SubmitProjectRequest request, CancellationToken ct = default);
    Task<GeneratedFile> GenerateDocxAsync(Guid id, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task<PagedResult<ProjectListItemResponse>> GetPagedAsync(
        int page, int pageSize,
        string? semesterId, string? status, string? search,
        CancellationToken ct = default);
    Task<ProjectResponse> GetByIdAsync(Guid id, CancellationToken ct = default);
}
