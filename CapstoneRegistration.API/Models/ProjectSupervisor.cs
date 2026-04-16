using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CapstoneRegistration.API.Models;

[Table("project_supervisors")]
public class ProjectSupervisor
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [Column("project_id")]
    public Guid ProjectId { get; set; }

    [Required]
    [Column("full_name")]
    [MaxLength(255)]
    public string FullName { get; set; } = null!;

    [Column("phone")]
    [MaxLength(20)]
    public string? Phone { get; set; }

    [Column("email")]
    [MaxLength(255)]
    public string? Email { get; set; }

    [Column("title")]
    [MaxLength(100)]
    public string? Title { get; set; }

    [Column("is_primary")]
    public bool IsPrimary { get; set; } = false;

    [Column("display_order")]
    public int DisplayOrder { get; set; } = 0;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(ProjectId))]
    public CapstoneProject Project { get; set; } = null!;
}
