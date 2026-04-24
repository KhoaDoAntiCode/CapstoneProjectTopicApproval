using CapstoneRegistration.API.Common;
using CapstoneRegistration.API.Models;

namespace CapstoneRegistration.API.Services.Interfaces;

public interface IProjectDocumentService
{
    Task<GeneratedFile> GenerateAsync(CapstoneProject project, CancellationToken ct = default);
}
