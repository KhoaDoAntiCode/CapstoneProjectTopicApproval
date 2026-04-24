using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CapstoneRegistration.API.Models;

[Table("students")]
public class ProjectStudent
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [Column("full_name")]
    [MaxLength(255)]
    public string FullName { get; set; } = null!;

    [Column("student_code")]
    [MaxLength(20)]
    public string? StudentCode { get; set; }

    [Column("phone")]
    [MaxLength(20)]
    public string? Phone { get; set; }

    [Column("email")]
    [MaxLength(255)]
    public string? Email { get; set; }

    [Column("major")]
    [MaxLength(100)]
    public string? Major { get; set; }

    [Column("specialty")]
    [MaxLength(50)]
    public string? Specialty { get; set; }

    [Column("active")]
    public bool Active { get; set; } = true;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [NotMapped]
    [MaxLength(20)]
    public string? RoleInGroup { get; set; }

    [NotMapped]
    public int DisplayOrder { get; set; } = 0;
}
