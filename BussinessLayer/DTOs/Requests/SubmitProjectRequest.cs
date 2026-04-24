using System.ComponentModel.DataAnnotations;

namespace CapstoneRegistration.API.DTOs.Requests;

public class SubmitProjectRequest
{
    [Required]
    public string SemesterId { get; set; } = null!;

    [Required]
    [MaxLength(500)]
    public string EnglishName { get; set; } = null!;

    [Required]
    [MaxLength(500)]
    public string VietnameseName { get; set; } = null!;

    [MaxLength(255)]
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

    [MaxLength(20)]
    public string? ClassName { get; set; }

    public DateOnly? DurationFrom { get; set; }
    public DateOnly? DurationTo { get; set; }

    [MaxLength(100)]
    public string? Profession { get; set; }

    [MaxLength(10)]
    public string? Specialty { get; set; }

    [MaxLength(20)]
    public string? RegisterKind { get; set; }

    public List<SupervisorRequest> Supervisors { get; set; } = [];
    public List<StudentRequest> Students { get; set; } = [];
}

public class SupervisorRequest
{
    [Required]
    [MaxLength(255)]
    public string FullName { get; set; } = null!;
    [MaxLength(20)]  public string? Phone { get; set; }
    [MaxLength(255)] public string? Email { get; set; }
    [MaxLength(100)] public string? Title { get; set; }
    public bool IsPrimary { get; set; }
    public int DisplayOrder { get; set; }
}

public class StudentRequest
{
    [Required]
    [MaxLength(255)]
    public string FullName { get; set; } = null!;
    [MaxLength(20)]  public string? StudentCode { get; set; }
    [MaxLength(20)]  public string? Phone { get; set; }
    [MaxLength(255)] public string? Email { get; set; }
    [MaxLength(10)]  public string? RoleInGroup { get; set; }
    public int DisplayOrder { get; set; }
}
