using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using CapstoneRegistration.API.Common;
using CapstoneRegistration.API.Models;
using CapstoneRegistration.API.Services.Interfaces;

namespace CapstoneRegistration.API.Services.Implementations;

public class ProjectDocumentService : IProjectDocumentService
{
    private const string DocxContentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";

    public Task<GeneratedFile> GenerateAsync(CapstoneProject project, CancellationToken ct = default)
    {
        using var stream = new MemoryStream();

        using (var document = WordprocessingDocument.Create(stream, WordprocessingDocumentType.Document, true))
        {
            var mainPart = document.AddMainDocumentPart();
            mainPart.Document = new Document();

            var body = new Body();
            mainPart.Document.Append(body);

            AddParagraph(body, "CAPSTONE PROJECT REGISTER", bold: true, fontSize: "32", justification: JustificationValues.Center);
            AddSpacer(body);

            AddParagraph(
                body,
                $"Class: {project.ClassName ?? "-"}    Duration time: from {FormatMonth(project.DurationFrom)} to {FormatMonth(project.DurationTo)}",
                fontSize: "24");
            AddParagraph(body, $"Profession: {project.Profession ?? "-"}    Specialty: {project.Specialty ?? "-"}", fontSize: "24");
            AddParagraph(body, $"Kinds of person make registers: {project.RegisterKind ?? "-"}", fontSize: "24");

            AddSpacer(body);
            AddParagraph(body, "1. Register information for supervisor (if have)", bold: true, fontSize: "26");
            body.Append(BuildSupervisorTable(project.Supervisors.OrderBy(x => x.DisplayOrder).ToList()));

            AddSpacer(body);
            AddParagraph(body, "2. Register information for students (if have)", bold: true, fontSize: "26");
            body.Append(BuildStudentTable(project.Students.OrderBy(x => x.DisplayOrder).ToList()));

            AddSpacer(body);
            AddParagraph(body, "3. Register content of Capstone Project", bold: true, fontSize: "26");
            AddParagraph(body, $"3.1. Capstone Project name", bold: true, fontSize: "24");
            AddParagraph(body, $"English: {project.EnglishName}", fontSize: "24");
            AddParagraph(body, $"Vietnamese: {project.VietnameseName}", fontSize: "24");
            AddParagraph(body, $"Abbreviation: {project.Abbreviation ?? "-"}", fontSize: "24");

            AddSection(body, "Context", project.Context);
            AddSection(body, "Proposed Solutions", project.ProposedSolutions);
            AddSection(body, "Functional Requirements", project.FunctionalRequirements);
            AddSection(body, "Non-functional Requirements", project.NonFunctionalRequirements);
            AddSection(body, "3.2. Main proposal content (including result and product)", project.TheoryAndPractice);
            AddSection(body, "Products (Expected Deliverables)", project.Products);
            AddSection(body, "Proposed Tasks", project.ProposedTasks);

            AddSpacer(body);
            AddParagraph(body, $"Status: {project.Status}", bold: true, fontSize: "24");
            AddParagraph(body, $"Generated at: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC", fontSize: "22");

            mainPart.Document.Save();
        }

        return Task.FromResult(new GeneratedFile
        {
            FileName = $"{project.ProjectCode}.docx",
            ContentType = DocxContentType,
            Content = stream.ToArray()
        });
    }

    private static void AddSection(Body body, string title, string? content)
    {
        AddParagraph(body, $"{title}:", bold: true, fontSize: "24");
        if (string.IsNullOrWhiteSpace(content))
        {
            AddParagraph(body, "-", fontSize: "22");
            return;
        }

        foreach (var line in content.Split(["\r\n", "\n"], StringSplitOptions.None))
        {
            AddParagraph(body, string.IsNullOrWhiteSpace(line) ? " " : line, fontSize: "22");
        }
    }

    private static Table BuildSupervisorTable(IReadOnlyList<ProjectSupervisor> supervisors)
    {
        var table = CreateTable(["No.", "Full name", "Phone", "E-Mail", "Title"]);

        if (supervisors.Count == 0)
        {
            table.Append(CreateRow(["Supervisor", "-", "-", "-", "-"]));
            return table;
        }

        foreach (var supervisor in supervisors)
        {
            table.Append(CreateRow([
                supervisor.IsPrimary ? "Supervisor" : string.Empty,
                supervisor.FullName,
                supervisor.Phone ?? string.Empty,
                supervisor.Email ?? string.Empty,
                supervisor.Title ?? string.Empty
            ]));
        }

        return table;
    }

    private static Table BuildStudentTable(IReadOnlyList<ProjectStudent> students)
    {
        var table = CreateTable(["No.", "Full name", "Student code", "Phone", "E-mail", "Role in Group"]);

        if (students.Count == 0)
        {
            table.Append(CreateRow(["1", "-", "-", "-", "-", "-"]));
            return table;
        }

        for (var i = 0; i < students.Count; i++)
        {
            var student = students[i];
            table.Append(CreateRow([
                (i + 1).ToString(),
                student.FullName,
                student.StudentCode ?? string.Empty,
                student.Phone ?? string.Empty,
                student.Email ?? string.Empty,
                student.RoleInGroup ?? string.Empty
            ]));
        }

        return table;
    }

    private static Table CreateTable(string[] headers)
    {
        var table = new Table();

        var properties = new TableProperties(
            new TableBorders(
                new TopBorder { Val = BorderValues.Single, Size = 8 },
                new BottomBorder { Val = BorderValues.Single, Size = 8 },
                new LeftBorder { Val = BorderValues.Single, Size = 8 },
                new RightBorder { Val = BorderValues.Single, Size = 8 },
                new InsideHorizontalBorder { Val = BorderValues.Single, Size = 8 },
                new InsideVerticalBorder { Val = BorderValues.Single, Size = 8 }),
            new TableWidth { Type = TableWidthUnitValues.Pct, Width = "5000" });

        table.AppendChild(properties);
        table.Append(CreateHeaderRow(headers));
        return table;
    }

    private static TableRow CreateHeaderRow(IEnumerable<string> cells)
    {
        var row = new TableRow();
        foreach (var cellValue in cells)
        {
            row.Append(CreateCell(cellValue, true));
        }

        return row;
    }

    private static TableRow CreateRow(IEnumerable<string> cells)
    {
        var row = new TableRow();
        foreach (var cellValue in cells)
        {
            row.Append(CreateCell(cellValue, false));
        }

        return row;
    }

    private static TableCell CreateCell(string text, bool bold)
    {
        var runProperties = new RunProperties();
        if (bold)
        {
            runProperties.Append(new Bold());
        }

        runProperties.Append(new FontSize { Val = "22" });

        var paragraph = new Paragraph(
            new ParagraphProperties(new SpacingBetweenLines { After = "80", Before = "80" }),
            new Run(runProperties, new Text(text ?? string.Empty) { Space = SpaceProcessingModeValues.Preserve }));

        var cell = new TableCell(paragraph);
        cell.Append(new TableCellProperties(
            new TableCellVerticalAlignment { Val = TableVerticalAlignmentValues.Center },
            new TableCellWidth { Type = TableWidthUnitValues.Auto }));
        return cell;
    }

    private static void AddParagraph(
        Body body,
        string text,
        bool bold = false,
        string fontSize = "22",
        JustificationValues? justification = null)
    {
        var runProperties = new RunProperties(new FontSize { Val = fontSize });
        if (bold)
        {
            runProperties.Append(new Bold());
        }

        var paragraph = new Paragraph(
            new ParagraphProperties(
                new Justification { Val = justification ?? JustificationValues.Left },
                new SpacingBetweenLines { After = "120" }),
            new Run(runProperties, new Text(text) { Space = SpaceProcessingModeValues.Preserve }));

        body.Append(paragraph);
    }

    private static void AddSpacer(Body body) => AddParagraph(body, " ", fontSize: "8");

    private static string FormatMonth(DateOnly? value) =>
        value.HasValue ? value.Value.ToString("MM/yyyy") : "-";
}
