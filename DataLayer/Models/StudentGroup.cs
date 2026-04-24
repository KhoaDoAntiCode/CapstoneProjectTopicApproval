using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CapstoneRegistration.API.Models;

[Table("student_groups")]
public class StudentGroup
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Column("group_code")]
    [MaxLength(30)]
    public string? GroupCode { get; set; }

    [Column("class_name")]
    [MaxLength(50)]
    public string? ClassName { get; set; }

    [Column("profession")]
    [MaxLength(100)]
    public string? Profession { get; set; }

    [Column("specialty")]
    [MaxLength(50)]
    public string? Specialty { get; set; }

    [Column("duration_from")]
    public DateOnly? DurationFrom { get; set; }

    [Column("duration_to")]
    public DateOnly? DurationTo { get; set; }

    [Column("created_by_admin_id")]
    public Guid CreatedByAdminId { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(CreatedByAdminId))]
    public User CreatedByAdmin { get; set; } = null!;

    public ICollection<StudentGroupMember> Members { get; set; } = new List<StudentGroupMember>();
}
