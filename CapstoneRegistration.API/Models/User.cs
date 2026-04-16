using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CapstoneRegistration.API.Models;

[Table("users")]
public class User
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [Column("email")]
    [MaxLength(255)]
    public string Email { get; set; } = null!;

    [Required]
    [Column("full_name")]
    [MaxLength(255)]
    public string FullName { get; set; } = null!;

    [Column("avatar_url")]
    [MaxLength(500)]
    public string? AvatarUrl { get; set; }

    [Required]
    [Column("role")]
    [MaxLength(20)]
    public string Role { get; set; } = null!;

    [Required]
    [Column("password_hash")]
    [MaxLength(255)]
    public string PasswordHash { get; set; } = null!;

    [Column("is_email_verified")]
    public bool IsEmailVerified { get; set; } = false;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<CapstoneProject> CreatedProjects { get; set; } = new List<CapstoneProject>();
    public ICollection<ProjectReview> Reviews { get; set; } = new List<ProjectReview>();
}
