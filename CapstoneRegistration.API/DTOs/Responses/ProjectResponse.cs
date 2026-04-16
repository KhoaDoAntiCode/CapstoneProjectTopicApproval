namespace CapstoneRegistration.API.DTOs.Responses;

public class ProjectResponse
{
    public Guid Id { get; set; }
    public string ProjectCode { get; set; } = null!;
    public string SemesterId { get; set; } = null!;
    public string EnglishName { get; set; } = null!;
    public string VietnameseName { get; set; } = null!;
    public string? Abbreviation { get; set; }
    public bool IsResearchProject { get; set; }
    public bool IsEnterpriseProject { get; set; }
    public string? Context { get; set; }
    public string? ProposedSolutions { get; set; }
    public string? FunctionalRequirements { get; set; }
    public string? NonFunctionalRequirements { get; set; }
    public string? TheoryAndPractice { get; set; }
    public string? Products { get; set; }
    public string? ProposedTasks { get; set; }
    public string? ClassName { get; set; }
    public DateOnly? DurationFrom { get; set; }
    public DateOnly? DurationTo { get; set; }
    public string? Profession { get; set; }
    public string? Specialty { get; set; }
    public string? RegisterKind { get; set; }
    public string Status { get; set; } = null!;
    public Guid CreatedById { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<SupervisorResponse> Supervisors { get; set; } = [];
    public List<StudentResponse> Students { get; set; } = [];
    public List<ReviewResponse> Reviews { get; set; } = [];
}

public class SupervisorResponse
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = null!;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Title { get; set; }
    public bool IsPrimary { get; set; }
    public int DisplayOrder { get; set; }
}

public class StudentResponse
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = null!;
    public string? StudentCode { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? RoleInGroup { get; set; }
    public int DisplayOrder { get; set; }
}
