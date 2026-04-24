using CapstoneRegistration.API.Common;
using CapstoneRegistration.API.Data;
using CapstoneRegistration.API.DTOs.Requests;
using CapstoneRegistration.API.DTOs.Responses;
using CapstoneRegistration.API.Exceptions;
using CapstoneRegistration.API.Models;
using CapstoneRegistration.API.Repositories.Interfaces;
using CapstoneRegistration.API.Services.Interfaces;

namespace CapstoneRegistration.API.Services.Implementations;

public class ProjectService : IProjectService
{
    private readonly ApplicationDbContext _db;
    private readonly ICapstoneProjectRepository _projectRepo;
    private readonly IDocxParserService _docxParser;
    private readonly IProjectDocumentService _projectDocumentService;

    public ProjectService(
        ApplicationDbContext db,
        ICapstoneProjectRepository projectRepo,
        IDocxParserService docxParser,
        IProjectDocumentService projectDocumentService)
    {
        _db           = db;
        _projectRepo  = projectRepo;
        _docxParser   = docxParser;
        _projectDocumentService = projectDocumentService;
    }

    public Task<DocxPreviewResponse> ParseDocxAsync(Stream docxStream, CancellationToken ct = default) =>
        _docxParser.ParseAsync(docxStream, ct);

    public async Task<ProjectResponse> SubmitAsync(
        Guid createdById,
        SubmitProjectRequest request,
        CancellationToken ct = default)
    {
        var projectCode = await _projectRepo.GenerateProjectCodeAsync(request.SemesterId, ct);

        var project = new CapstoneProject
        {
            ProjectCode               = projectCode,
            CreatedById               = createdById,
            Status                    = "Pending"
        };

        ApplyRequest(project, request);
        await _projectRepo.AddAsync(project, ct);

        var saved = await _projectRepo.GetByIdWithDetailsAsync(project.Id, ct)
            ?? throw new NotFoundException("CapstoneProject", project.Id);

        return MapToResponse(saved);
    }

    public async Task<ProjectResponse> UpdateAsync(
        Guid id,
        SubmitProjectRequest request,
        CancellationToken ct = default)
    {
        var project = await _projectRepo.GetByIdWithDetailsAsync(id, ct)
            ?? throw new NotFoundException("CapstoneProject", id);

        _db.ProjectSupervisors.RemoveRange(project.Supervisors);
        _db.ProjectStudents.RemoveRange(project.Students);

        project.Supervisors.Clear();
        project.Students.Clear();

        ApplyRequest(project, request);
        project.Status = "Updated";
        project.UpdatedAt = DateTime.UtcNow;

        await _projectRepo.UpdateAsync(project, ct);

        var saved = await _projectRepo.GetByIdWithDetailsAsync(id, ct)
            ?? throw new NotFoundException("CapstoneProject", id);

        return MapToResponse(saved);
    }

    public async Task<GeneratedFile> CreateWithDocxAsync(
        Guid createdById,
        SubmitProjectRequest request,
        CancellationToken ct = default)
    {
        var project = await SubmitAsync(createdById, request, ct);
        return await GenerateDocxAsync(project.Id, ct);
    }

    public async Task<GeneratedFile> GenerateDocxAsync(Guid id, CancellationToken ct = default)
    {
        var project = await _projectRepo.GetByIdWithDetailsAsync(id, ct)
            ?? throw new NotFoundException("CapstoneProject", id);

        return await _projectDocumentService.GenerateAsync(project, ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var project = await _projectRepo.GetByIdWithDetailsAsync(id, ct)
            ?? throw new NotFoundException("CapstoneProject", id);

        await _projectRepo.DeleteAsync(project, ct);
    }

    public async Task<PagedResult<ProjectListItemResponse>> GetPagedAsync(
        int page, int pageSize,
        string? semesterId, string? status, string? search,
        CancellationToken ct = default)
    {
        var paged = await _projectRepo.GetPagedAsync(page, pageSize, semesterId, status, search, ct);
        var items = paged.Items.Select(p => new ProjectListItemResponse
        {
            Id             = p.Id,
            ProjectCode    = p.ProjectCode,
            SemesterId     = p.SemesterId,
            EnglishName    = p.EnglishName,
            VietnameseName = p.VietnameseName,
            Specialty      = p.Specialty,
            Status         = p.Status,
            CreatedByName  = p.CreatedBy.FullName,
            CreatedAt      = p.CreatedAt
        }).ToList();

        return PagedResult<ProjectListItemResponse>.Create(items, paged.TotalCount, page, pageSize);
    }

    public async Task<ProjectResponse> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var project = await _projectRepo.GetByIdWithDetailsAsync(id, ct)
            ?? throw new NotFoundException("CapstoneProject", id);
        return MapToResponse(project);
    }

    private static void ApplyRequest(CapstoneProject project, SubmitProjectRequest request)
    {
        project.SemesterId = request.SemesterId;
        project.EnglishName = request.EnglishName;
        project.VietnameseName = request.VietnameseName;
        project.Abbreviation = request.Abbreviation;
        project.IsResearchProject = request.IsResearchProject;
        project.IsEnterpriseProject = request.IsEnterpriseProject;
        project.Context = request.Context;
        project.ProposedSolutions = request.ProposedSolutions;
        project.FunctionalRequirements = request.FunctionalRequirements;
        project.NonFunctionalRequirements = request.NonFunctionalRequirements;
        project.TheoryAndPractice = request.TheoryAndPractice;
        project.Products = request.Products;
        project.ProposedTasks = request.ProposedTasks;
        project.ClassName = request.ClassName;
        project.DurationFrom = request.DurationFrom;
        project.DurationTo = request.DurationTo;
        project.Profession = request.Profession;
        project.Specialty = request.Specialty;
        project.RegisterKind = request.RegisterKind;
        project.UpdatedAt = DateTime.UtcNow;

        project.Supervisors = request.Supervisors.Select((s, i) => new ProjectSupervisor
        {
            FullName = s.FullName,
            Phone = s.Phone,
            Email = s.Email,
            Title = s.Title,
            IsPrimary = s.IsPrimary,
            DisplayOrder = s.DisplayOrder > 0 ? s.DisplayOrder : i + 1
        }).ToList();

        project.Students = request.Students.Select((s, i) => new ProjectStudent
        {
            FullName = s.FullName,
            StudentCode = s.StudentCode,
            Phone = s.Phone,
            Email = s.Email,
            RoleInGroup = s.RoleInGroup,
            DisplayOrder = s.DisplayOrder > 0 ? s.DisplayOrder : i + 1
        }).ToList();
    }

    internal static ProjectResponse MapToResponse(CapstoneProject p) => new()
    {
        Id                        = p.Id,
        ProjectCode               = p.ProjectCode,
        SemesterId                = p.SemesterId,
        EnglishName               = p.EnglishName,
        VietnameseName            = p.VietnameseName,
        Abbreviation              = p.Abbreviation,
        IsResearchProject         = p.IsResearchProject,
        IsEnterpriseProject       = p.IsEnterpriseProject,
        Context                   = p.Context,
        ProposedSolutions         = p.ProposedSolutions,
        FunctionalRequirements    = p.FunctionalRequirements,
        NonFunctionalRequirements = p.NonFunctionalRequirements,
        TheoryAndPractice         = p.TheoryAndPractice,
        Products                  = p.Products,
        ProposedTasks             = p.ProposedTasks,
        ClassName                 = p.ClassName,
        DurationFrom              = p.DurationFrom,
        DurationTo                = p.DurationTo,
        Profession                = p.Profession,
        Specialty                 = p.Specialty,
        RegisterKind              = p.RegisterKind,
        Status                    = p.Status,
        CreatedById               = p.CreatedById,
        CreatedAt                 = p.CreatedAt,
        UpdatedAt                 = p.UpdatedAt,
        Supervisors = p.Supervisors.Select(s => new SupervisorResponse
        {
            Id           = s.Id,
            FullName     = s.FullName,
            Phone        = s.Phone,
            Email        = s.Email,
            Title        = s.Title,
            IsPrimary    = s.IsPrimary,
            DisplayOrder = s.DisplayOrder
        }).ToList(),
        Students = p.Students.Select(s => new StudentResponse
        {
            Id           = s.Id,
            FullName     = s.FullName,
            StudentCode  = s.StudentCode,
            Phone        = s.Phone,
            Email        = s.Email,
            RoleInGroup  = s.RoleInGroup,
            DisplayOrder = s.DisplayOrder
        }).ToList(),
        Reviews = p.ProjectReviews.Select(r => new ReviewResponse
        {
            Id             = r.Id,
            ProjectId      = r.ProjectId,
            ReviewedById   = r.ReviewedById,
            ReviewedByName = r.ReviewedBy.FullName,
            Decision       = r.Decision,
            Comment        = r.Comment,
            ReviewedAt     = r.ReviewedAt
        }).ToList()
    };
}
