using CapstoneRegistration.API.DTOs.Responses;

namespace CapstoneRegistration.API.Services.Interfaces;

public interface IDocxParserService
{
    Task<DocxPreviewResponse> ParseAsync(Stream docxStream, CancellationToken ct = default);
}
