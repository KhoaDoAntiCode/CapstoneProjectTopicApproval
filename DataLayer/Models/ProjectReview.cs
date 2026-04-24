using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CapstoneRegistration.API.Models;

[Table("project_reviews")]
public class ProjectReview
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [Column("project_id")]
    public Guid ProjectId { get; set; }

    [Required]
    [Column("reviewed_by_id")]
    public Guid ReviewedById { get; set; }

    [Required]
    [Column("decision")]
    [MaxLength(20)]
    public string Decision { get; set; } = null!;

    [Column("comment")]
    public string? Comment { get; set; }

    [Column("reviewed_at")]
    public DateTime ReviewedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(ProjectId))]
    public CapstoneProject Project { get; set; } = null!;

    [ForeignKey(nameof(ReviewedById))]
    public User ReviewedBy { get; set; } = null!;
}
