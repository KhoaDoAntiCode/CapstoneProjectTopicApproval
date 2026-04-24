using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CapstoneRegistration.API.Models;

[Table("capstone_submissions")]
public class CapstoneProject
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [Column("submission_code")]
    [MaxLength(30)]
    public string ProjectCode { get; set; } = null!;

    [Required]
    [Column("group_id")]
    public Guid GroupId { get; set; }

    [Column("supervisor_id")]
    public Guid? SupervisorId { get; set; }

    [Required]
    [Column("created_by_admin_id")]
    public Guid CreatedById { get; set; }

    [Required]
    [Column("topic_name_en")]
    [MaxLength(500)]
    public string EnglishName { get; set; } = null!;

    [Required]
    [Column("topic_name_vi")]
    [MaxLength(500)]
    public string VietnameseName { get; set; } = null!;

    [Column("abbreviation")]
    [MaxLength(255)]
    public string? Abbreviation { get; set; }

    [NotMapped]
    public bool IsResearchProject { get; set; } = false;

    [NotMapped]
    public bool IsEnterpriseProject { get; set; } = false;

    [Column("context")]
    public string? Context { get; set; }

    [Column("server_side_technologies")]
    public string? ProposedSolutions { get; set; }

    [Column("functional_requirements")]
    public string? FunctionalRequirements { get; set; }

    [Column("non_functional_requirements")]
    public string? NonFunctionalRequirements { get; set; }

    [Column("main_proposal_content")]
    public string? TheoryAndPractice { get; set; }

    [Column("expected_deliverables")]
    public string? Products { get; set; }

    [Column("proposed_tasks")]
    public string? ProposedTasks { get; set; }

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

    [ForeignKey(nameof(GroupId))]
    public StudentGroup Group { get; set; } = null!;

    [ForeignKey(nameof(SupervisorId))]
    public ProjectSupervisor? Supervisor { get; set; }

    [NotMapped]
    public string SemesterId
    {
        get => ProjectCode.Length >= 4 ? ProjectCode[..4] : ProjectCode;
        set { }
    }

    [NotMapped]
    public string? ClassName
    {
        get => Group?.ClassName;
        set
        {
            if (Group != null)
            {
                Group.ClassName = value;
            }
        }
    }

    [NotMapped]
    public DateOnly? DurationFrom
    {
        get => Group?.DurationFrom;
        set
        {
            if (Group != null)
            {
                Group.DurationFrom = value;
            }
        }
    }

    [NotMapped]
    public DateOnly? DurationTo
    {
        get => Group?.DurationTo;
        set
        {
            if (Group != null)
            {
                Group.DurationTo = value;
            }
        }
    }

    [NotMapped]
    public string? Profession
    {
        get => Group?.Profession;
        set
        {
            if (Group != null)
            {
                Group.Profession = value;
            }
        }
    }

    [NotMapped]
    public string? Specialty
    {
        get => Group?.Specialty;
        set
        {
            if (Group != null)
            {
                Group.Specialty = value;
            }
        }
    }
}
