namespace CapstoneRegistration.API.DTOs.Responses;

public class DocxPreviewResponse
{
    public string? DetectedSemesterId { get; set; }
    public string? EnglishName { get; set; }
    public string? VietnameseName { get; set; }
    public string? Abbreviation { get; set; }
    public string? ClassName { get; set; }
    public DateOnly? DurationFrom { get; set; }
    public DateOnly? DurationTo { get; set; }
    public string? Profession { get; set; }
    public string? Specialty { get; set; }
    public string? RegisterKind { get; set; }
    public string? Context { get; set; }
    public string? ProposedSolutions { get; set; }
    public string? FunctionalRequirements { get; set; }
    public string? NonFunctionalRequirements { get; set; }
    public string? TheoryAndPractice { get; set; }
    public string? Products { get; set; }
    public string? ProposedTasks { get; set; }
    public List<SupervisorResponse> Supervisors { get; set; } = [];
    public List<StudentResponse> Students { get; set; } = [];
}
