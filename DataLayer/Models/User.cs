using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CapstoneRegistration.API.Models;

[Table("admin_users")]
public class User
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [Column("username")]
    [MaxLength(100)]
    public string Username { get; set; } = null!;

    [Required]
    [Column("email")]
    [MaxLength(255)]
    public string Email { get; set; } = null!;

    [Required]
    [Column("full_name")]
    [MaxLength(255)]
    public string FullName { get; set; } = null!;

    [Required]
    [Column("password")]
    [MaxLength(255)]
    public string Password { get; set; } = null!;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [NotMapped]
    public string Role => "Admin";

    public ICollection<CapstoneProject> CreatedProjects { get; set; } = new List<CapstoneProject>();
}
