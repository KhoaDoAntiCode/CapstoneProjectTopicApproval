using System.ComponentModel.DataAnnotations;

namespace CapstoneRegistration.API.DTOs.Requests;

public class CreateInstructorRequest
{
    [Required]
    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    [Phone]
    [MaxLength(20)]
    public string? PhoneNumber { get; set; }

    [MaxLength(50)]
    public string? Pronouns { get; set; }
}
