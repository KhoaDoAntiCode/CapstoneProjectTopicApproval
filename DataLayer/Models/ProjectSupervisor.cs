using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CapstoneRegistration.API.Models;

[Table("instructors")]
public class ProjectSupervisor
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

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

    [Column("department")]
    [MaxLength(150)]
    public string? Department { get; set; }

    [Column("active")]
    public bool Active { get; set; } = true;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [NotMapped]
    public bool IsPrimary { get; set; } = true;

    [NotMapped]
    public int DisplayOrder { get; set; } = 1;
}
