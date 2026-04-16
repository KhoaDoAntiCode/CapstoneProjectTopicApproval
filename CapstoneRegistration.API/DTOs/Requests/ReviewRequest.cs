using System.ComponentModel.DataAnnotations;

namespace CapstoneRegistration.API.DTOs.Requests;

public class ReviewRequest
{
    [Required]
    public string Decision { get; set; } = null!;

    public string? Comment { get; set; }
}
