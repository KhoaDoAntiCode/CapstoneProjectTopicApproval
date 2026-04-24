namespace CapstoneRegistration.API.DTOs.Responses;

public class ProjectListItemResponse
{
    public Guid Id { get; set; }
    public string ProjectCode { get; set; } = null!;
    public string SemesterId { get; set; } = null!;
    public string EnglishName { get; set; } = null!;
    public string VietnameseName { get; set; } = null!;
    public string? Specialty { get; set; }
    public string Status { get; set; } = null!;
    public string CreatedByName { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}
