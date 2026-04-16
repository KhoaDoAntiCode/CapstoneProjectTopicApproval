using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CapstoneRegistration.API.Models;

[Table("semesters")]
public class Semester
{
    [Key]
    [Column("id")]
    [MaxLength(20)]
    public string Id { get; set; } = null!; // e.g. "SU26"

    [Required]
    [Column("name")]
    [MaxLength(100)]
    public string Name { get; set; } = null!;

    [Column("start_date")]
    public DateOnly StartDate { get; set; }

    [Column("end_date")]
    public DateOnly EndDate { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; } = false;

    // Navigation
    public ICollection<CapstoneProject> CapstoneProjects { get; set; } = new List<CapstoneProject>();
}
