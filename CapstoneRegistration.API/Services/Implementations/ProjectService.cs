using CapstoneRegistration.API.Common;
using CapstoneRegistration.API.DTOs.Requests;
using CapstoneRegistration.API.DTOs.Responses;
using CapstoneRegistration.API.Exceptions;
using CapstoneRegistration.API.Models;
using CapstoneRegistration.API.Repositories.Interfaces;
using CapstoneRegistration.API.Services.Interfaces;

namespace CapstoneRegistration.API.Services.Implementations;

public class ProjectService : IProjectService
{
    private readonly ICapstoneProjectRepository _projectRepo;
    private readonly IDocxParserService _docxParser;

    public ProjectService(
        ICapstoneProjectRepository projectRepo,
        IDocxParserService docxParser)
    {
        _projectRepo  = projectRepo;
        _docxParser   = docxParser;
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
            SemesterId                = request.SemesterId,
            CreatedById               = createdById,
            EnglishName               = request.EnglishName,
            VietnameseName            = request.VietnameseName,
            Abbreviation              = request.Abbreviation,
            IsResearchProject         = request.IsResearchProject,
            IsEnterpriseProject       = request.IsEnterpriseProject,
            Context                   = request.Context,
            ProposedSolutions         = request.ProposedSolutions,
            FunctionalRequirements    = request.FunctionalRequirements,
            NonFunctionalRequirements = request.NonFunctionalRequirements,
            TheoryAndPractice         = request.TheoryAndPractice,
            Products                  = request.Products,
            ProposedTasks             = request.ProposedTasks,
            ClassName                 = request.ClassName,
            DurationFrom              = request.DurationFrom,
            DurationTo                = request.DurationTo,
            Profession                = request.Profession,
            Specialty                 = request.Specialty,
            RegisterKind              = request.RegisterKind,
            Status                    = "Pending",
            Supervisors = request.Supervisors.Select((s, i) => new ProjectSupervisor
            {
                FullName     = s.FullName,
                Phone        = s.Phone,
                Email        = s.Email,
                Title        = s.Title,
                IsPrimary    = s.IsPrimary,
                DisplayOrder = s.DisplayOrder > 0 ? s.DisplayOrder : i
            }).ToList(),
            Students = request.Students.Select((s, i) => new ProjectStudent
            {
                FullName     = s.FullName,
                StudentCode  = s.StudentCode,
                Phone        = s.Phone,
                Email        = s.Email,
                RoleInGroup  = s.RoleInGroup,
                DisplayOrder = s.DisplayOrder > 0 ? s.DisplayOrder : i
            }).ToList()
        };

        await _projectRepo.AddAsync(project, ct);

        var saved = await _projectRepo.GetByIdWithDetailsAsync(project.Id, ct)
            ?? throw new NotFoundException("CapstoneProject", project.Id);

        return MapToResponse(saved);
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
