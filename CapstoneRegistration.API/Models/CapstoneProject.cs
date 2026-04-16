using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CapstoneRegistration.API.Models;

[Table("capstone_projects")]
public class CapstoneProject
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [Column("project_code")]
    [MaxLength(20)]
    public string ProjectCode { get; set; } = null!;

    [Required]
    [Column("semester_id")]
    [MaxLength(20)]
    public string SemesterId { get; set; } = null!;

    [Required]
    [Column("created_by_id")]
    public Guid CreatedById { get; set; }

    [Required]
    [Column("english_name")]
    [MaxLength(500)]
    public string EnglishName { get; set; } = null!;

    [Required]
    [Column("vietnamese_name")]
    [MaxLength(500)]
    public string VietnameseName { get; set; } = null!;

    [Column("abbreviation")]
    [MaxLength(255)]
    public string? Abbreviation { get; set; }

    [Column("is_research_project")]
    public bool IsResearchProject { get; set; } = false;

    [Column("is_enterprise_project")]
    public bool IsEnterpriseProject { get; set; } = false;

    [Column("context")]
    public string? Context { get; set; }

    [Column("proposed_solutions")]
    public string? ProposedSolutions { get; set; }

    [Column("functional_requirements")]
    public string? FunctionalRequirements { get; set; }

    [Column("non_functional_requirements")]
    public string? NonFunctionalRequirements { get; set; }

    [Column("theory_and_practice")]
    public string? TheoryAndPractice { get; set; }

    [Column("products")]
    public string? Products { get; set; }

    [Column("proposed_tasks")]
    public string? ProposedTasks { get; set; }

    [Column("class")]
    [MaxLength(20)]
    public string? ClassName { get; set; }

    [Column("duration_from")]
    public DateOnly? DurationFrom { get; set; }

    [Column("duration_to")]
    public DateOnly? DurationTo { get; set; }

    [Column("profession")]
    [MaxLength(100)]
    public string? Profession { get; set; }

    [Column("specialty")]
    [MaxLength(10)]
    public string? Specialty { get; set; }

    [Column("register_kind")]
    [MaxLength(20)]
    public string? RegisterKind { get; set; }

    [Required]
    [Column("status")]
    [MaxLength(20)]
    public string Status { get; set; } = "Pending";

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(CreatedById))]
    public User CreatedBy { get; set; } = null!;

    public ICollection<ProjectSupervisor> Supervisors { get; set; } = new List<ProjectSupervisor>();
    public ICollection<ProjectStudent> Students { get; set; } = new List<ProjectStudent>();
    public ICollection<ProjectReview> ProjectReviews { get; set; } = new List<ProjectReview>();
}
