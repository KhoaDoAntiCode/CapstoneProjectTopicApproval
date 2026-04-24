using CapstoneRegistration.API.Common;
using CapstoneRegistration.API.DTOs.Requests;
using CapstoneRegistration.API.DTOs.Responses;
using CapstoneRegistration.API.Exceptions;
using CapstoneRegistration.API.Models;
using CapstoneRegistration.API.Services.Interfaces;
using CapstoneRegistration.API.UnitOfWorks;

namespace CapstoneRegistration.API.Services.Implementations;

public class ProjectService : IProjectService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDocxParserService _docxParser;
    private readonly IProjectDocumentService _projectDocumentService;

    public ProjectService(
        IUnitOfWork unitOfWork,
        IDocxParserService docxParser,
        IProjectDocumentService projectDocumentService)
    {
        _unitOfWork   = unitOfWork;
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
        var projectCode = await _unitOfWork.CapstoneProjects.GenerateProjectCodeAsync(request.SemesterId, ct);

        var project = new CapstoneProject
        {
            ProjectCode               = projectCode,
            CreatedById               = createdById,
            Status                    = "submitted",
            Group                     = new StudentGroup { CreatedByAdminId = createdById }
        };

        ApplyRequest(project, request, createdById);
        await _unitOfWork.CapstoneProjects.AddAsync(project, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        var saved = await _unitOfWork.CapstoneProjects.GetByIdWithDetailsAsync(project.Id, ct)
            ?? throw new NotFoundException("CapstoneProject", project.Id);

        return MapToResponse(saved);
    }

    public async Task<ProjectResponse> UpdateAsync(
        Guid id,
        SubmitProjectRequest request,
        CancellationToken ct = default)
    {
        var project = await _unitOfWork.CapstoneProjects.GetByIdWithDetailsAsync(id, ct)
            ?? throw new NotFoundException("CapstoneProject", id);

        if (project.Supervisor is not null)
        {
            _unitOfWork.Context.ProjectSupervisors.Remove(project.Supervisor);
            project.Supervisor = null;
            project.SupervisorId = null;
        }

        if (project.Group is not null)
        {
            var existingMembers = project.Group.Members.ToList();
            var existingStudents = existingMembers.Select(m => m.Student).ToList();
            _unitOfWork.Context.StudentGroupMembers.RemoveRange(existingMembers);
            _unitOfWork.Context.ProjectStudents.RemoveRange(existingStudents);
            project.Group.Members.Clear();
        }

        ApplyRequest(project, request, project.CreatedById);
        project.Status = "updated";
        project.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.CapstoneProjects.UpdateAsync(project, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        var saved = await _unitOfWork.CapstoneProjects.GetByIdWithDetailsAsync(id, ct)
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
        var project = await _unitOfWork.CapstoneProjects.GetByIdWithDetailsAsync(id, ct)
            ?? throw new NotFoundException("CapstoneProject", id);

        return await _projectDocumentService.GenerateAsync(project, ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var project = await _unitOfWork.CapstoneProjects.GetByIdWithDetailsAsync(id, ct)
            ?? throw new NotFoundException("CapstoneProject", id);

        await _unitOfWork.CapstoneProjects.DeleteAsync(project, ct);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task<PagedResult<ProjectListItemResponse>> GetPagedAsync(
        int page, int pageSize,
        string? semesterId, string? status, string? search,
        CancellationToken ct = default)
    {
        var paged = await _unitOfWork.CapstoneProjects.GetPagedAsync(page, pageSize, semesterId, status, search, ct);
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
        var project = await _unitOfWork.CapstoneProjects.GetByIdWithDetailsAsync(id, ct)
            ?? throw new NotFoundException("CapstoneProject", id);
        return MapToResponse(project);
    }

    private static void ApplyRequest(CapstoneProject project, SubmitProjectRequest request, Guid createdById)
    {
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
        project.RegisterKind = request.RegisterKind;
        project.UpdatedAt = DateTime.UtcNow;
        project.Group ??= new StudentGroup { CreatedByAdminId = createdById };
        project.Group.CreatedByAdminId = createdById;
        project.Group.ClassName = request.ClassName;
        project.Group.DurationFrom = request.DurationFrom;
        project.Group.DurationTo = request.DurationTo;
        project.Group.Profession = request.Profession;
        project.Group.Specialty = request.Specialty;
        project.Group.UpdatedAt = DateTime.UtcNow;
        project.Group.GroupCode = request.ClassName;

        var primarySupervisor = request.Supervisors
            .OrderByDescending(s => s.IsPrimary)
            .ThenBy(s => s.DisplayOrder)
            .FirstOrDefault();

        if (primarySupervisor is not null)
        {
            project.Supervisor = new ProjectSupervisor
            {
                FullName = primarySupervisor.FullName,
                Phone = primarySupervisor.Phone,
                Email = primarySupervisor.Email,
                Title = primarySupervisor.Title,
                UpdatedAt = DateTime.UtcNow
            };
        }

        project.Group.Members = request.Students.Select((s, i) =>
        {
            var student = new ProjectStudent
            {
                FullName = s.FullName,
                StudentCode = s.StudentCode,
                Phone = s.Phone,
                Email = s.Email,
                Specialty = request.Specialty,
                UpdatedAt = DateTime.UtcNow
            };

            return new StudentGroupMember
            {
                Student = student,
                RoleInGroup = string.IsNullOrWhiteSpace(s.RoleInGroup) ? "Member" : s.RoleInGroup,
                DisplayOrder = s.DisplayOrder > 0 ? s.DisplayOrder : i + 1
            };
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
        Supervisors = p.Supervisor is null
            ? []
            : [
                new SupervisorResponse
                {
                    Id = p.Supervisor.Id,
                    FullName = p.Supervisor.FullName,
                    Phone = p.Supervisor.Phone,
                    Email = p.Supervisor.Email,
                    Title = p.Supervisor.Title,
                    IsPrimary = true,
                    DisplayOrder = 1
                }
            ],
        Students = p.Group?.Members
            .OrderBy(m => m.DisplayOrder)
            .Select(m => new StudentResponse
        {
            Id           = m.Student.Id,
            FullName     = m.Student.FullName,
            StudentCode  = m.Student.StudentCode,
            Phone        = m.Student.Phone,
            Email        = m.Student.Email,
            RoleInGroup  = m.RoleInGroup,
            DisplayOrder = m.DisplayOrder
        }).ToList() ?? [],
        Reviews = []
    };
}
