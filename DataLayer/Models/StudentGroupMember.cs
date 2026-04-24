using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CapstoneRegistration.API.Models;

[Table("student_group_members")]
public class StudentGroupMember
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Column("group_id")]
    public Guid GroupId { get; set; }

    [Column("student_id")]
    public Guid StudentId { get; set; }

    [Column("role_in_group")]
    [MaxLength(20)]
    public string RoleInGroup { get; set; } = "Member";

    [Column("display_order")]
    public int DisplayOrder { get; set; } = 1;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(GroupId))]
    public StudentGroup Group { get; set; } = null!;

    [ForeignKey(nameof(StudentId))]
    public ProjectStudent Student { get; set; } = null!;
}
