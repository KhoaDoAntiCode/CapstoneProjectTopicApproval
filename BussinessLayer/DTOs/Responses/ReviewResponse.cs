namespace CapstoneRegistration.API.DTOs.Responses;

public class ReviewResponse
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public Guid ReviewedById { get; set; }
    public string ReviewedByName { get; set; } = null!;
    public string Decision { get; set; } = null!;
    public string? Comment { get; set; }
    public DateTime ReviewedAt { get; set; }
}
