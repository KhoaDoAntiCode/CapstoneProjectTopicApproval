using System.Globalization;
using System.Text.RegularExpressions;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using CapstoneRegistration.API.Common;
using CapstoneRegistration.API.DTOs.Responses;
using CapstoneRegistration.API.Services.Interfaces;

using WpDrawing = DocumentFormat.OpenXml.Wordprocessing.Drawing;
using ABlip     = DocumentFormat.OpenXml.Drawing.Blip;

namespace CapstoneRegistration.API.Services.Implementations;

public partial class DocxParserService : IDocxParserService
{

    private const int CheckedThresholdBytes = 200;

    public Task<DocxPreviewResponse> ParseAsync(Stream docxStream, CancellationToken ct = default)
    {
        var response = new DocxPreviewResponse();

        using var doc = WordprocessingDocument.Open(docxStream, isEditable: false);
        var mainPart = doc.MainDocumentPart!;
        var body     = mainPart.Document!.Body!;

        var imageSizes = BuildImageSizeMap(mainPart);

        var topParas = body.ChildElements.OfType<Paragraph>().ToList();

        var tables = body.ChildElements.OfType<Table>().ToList();

        foreach (var para in topParas)
        {
            var text = GetText(para);
            if (text.Contains("Duration time", StringComparison.OrdinalIgnoreCase))
                ParseClassDuration(text, response);
            else if (text.Contains("Profession:", StringComparison.OrdinalIgnoreCase))
                ParseProfessionSpecialty(para, imageSizes, response);
            else if (text.Contains("Kinds of person make registers", StringComparison.OrdinalIgnoreCase))
                ParseRegisterKind(para, imageSizes, response);
        }

        ParseContentSections(topParas, response);

        if (tables.Count > 0) response.Supervisors = ParseSupervisorsTable(tables[0]);
        if (tables.Count > 1) response.Students    = ParseStudentsTable(tables[1]);

        if (response.DurationFrom.HasValue)
            response.DetectedSemesterId = SemesterHelper.ComputeId(response.DurationFrom.Value);

        return Task.FromResult(response);
    }

    private static void ParseClassDuration(string text, DocxPreviewResponse r)
    {
        var cm = ClassNameRegex().Match(text);
        if (cm.Success)
        {
            var cn = cm.Groups[1].Value.Trim();
            if (!string.IsNullOrWhiteSpace(cn) && !IsPlaceholder(cn))
                r.ClassName = cn;
        }

        var dm = DurationRegex().Match(text);
        if (dm.Success)
        {
            r.DurationFrom = TryParseDate(dm.Groups[1].Value);
            r.DurationTo   = TryParseDate(dm.Groups[2].Value);
        }
    }

    private static void ParseProfessionSpecialty(
        Paragraph para, Dictionary<string, int> imageSizes, DocxPreviewResponse r)
    {
        var fullText = GetText(para);

        var pm = ProfessionRegex().Match(fullText);
        if (pm.Success)
        {
            var prof = pm.Groups[1].Value.Trim().Trim('<', '>');
            if (!string.IsNullOrWhiteSpace(prof) && !IsPlaceholder(prof))
                r.Profession = prof;
        }

        string? lastOption = null;
        foreach (var element in para.ChildElements)
        {
            if (element is not Run run) continue;

            var t = GetText(run);
            if      (t.Contains("<ES>", StringComparison.OrdinalIgnoreCase)) lastOption = "ES";
            else if (t.Contains("<IS>", StringComparison.OrdinalIgnoreCase)) lastOption = "IS";
            else if (t.Contains("<JS>", StringComparison.OrdinalIgnoreCase)) lastOption = "JS";

            var drawing = run.GetFirstChild<WpDrawing>();
            if (drawing != null)
            {
                if (lastOption != null && IsChecked(drawing, imageSizes))
                    r.Specialty = lastOption;
                lastOption = null;
            }
        }
    }

    private static void ParseRegisterKind(
        Paragraph para, Dictionary<string, int> imageSizes, DocxPreviewResponse r)
    {
        string? lastOption = null;
        foreach (var element in para.ChildElements)
        {
            if (element is not Run run) continue;

            var t = GetText(run);
            if      (t.Contains("Lecturer", StringComparison.OrdinalIgnoreCase)) lastOption = "Lecturer";
            else if (t.Contains("Students", StringComparison.OrdinalIgnoreCase)) lastOption = "Students";

            var drawing = run.GetFirstChild<WpDrawing>();
            if (drawing != null)
            {
                if (lastOption != null && IsChecked(drawing, imageSizes))
                    r.RegisterKind = lastOption;
                lastOption = null;
            }
        }
    }

    private static void ParseContentSections(List<Paragraph> paragraphs, DocxPreviewResponse r)
    {
        const int None       = 0;
        const int Context    = 1;
        const int Proposed   = 2;
        const int FuncReq    = 3;
        const int NonFuncReq = 4;
        const int Theory     = 5;
        const int Products   = 6;
        const int Tasks      = 7;

        var section = None;
        var acc     = new List<string>();

        void Flush()
        {
            var content = string.Join("\n", acc.Where(s => !string.IsNullOrWhiteSpace(s))).Trim();
            if (!string.IsNullOrEmpty(content))
                switch (section)
                {
                    case Context:    r.Context                 = content; break;
                    case Proposed:   r.ProposedSolutions       = content; break;
                    case FuncReq:    r.FunctionalRequirements  = content; break;
                    case NonFuncReq: r.NonFunctionalRequirements = content; break;
                    case Theory:     r.TheoryAndPractice       = content; break;
                    case Products:   r.Products                = content; break;
                    case Tasks:      r.ProposedTasks           = content; break;
                }
            acc.Clear();
        }

        foreach (var para in paragraphs)
        {
            var text = GetText(para).Trim();
            if (string.IsNullOrWhiteSpace(text)) continue;

            var low = text.ToLowerInvariant();

            if (low.Contains("capstone project register") ||
                low.StartsWith("1. register") ||
                low.StartsWith("2. register") ||
                low.StartsWith("3. register") ||
                low.Contains("3.1.")          ||
                low.Contains("3.2.")          ||
                low.Contains("duration time") ||
                low.Contains("profession:")   ||
                low.Contains("kinds of person"))
                continue;

            if (low.StartsWith("english:"))    { r.EnglishName     = AfterColon(text); section = None; continue; }
            if (low.StartsWith("vietnamese:")) { r.VietnameseName  = AfterColon(text); section = None; continue; }
            if (low.StartsWith("abbreviation:")){ r.Abbreviation   = AfterColon(text); section = None; continue; }

            if (low.StartsWith("context:"))             { Flush(); section = Context;    continue; }
            if (low.Contains("proposed solution"))      { Flush(); section = Proposed;   continue; }
            if (low.Contains("non-functional require")) { Flush(); section = NonFuncReq; continue; }
            if (low.Contains("functional require"))     { Flush(); section = FuncReq;    continue; }
            if (low.Contains("theory and practice"))    { Flush(); section = Theory;     continue; }
            if (low.StartsWith("products:"))            { Flush(); section = Products;   continue; }
            if (low.StartsWith("proposed tasks:"))      { Flush(); section = Tasks;      continue; }

            if (low.StartsWith("4.")) { Flush(); section = None; break; }

            if (section != None)
                acc.Add(text);
        }

        Flush();
    }

    private static List<SupervisorResponse> ParseSupervisorsTable(Table table)
    {
        var result = new List<SupervisorResponse>();
        int order  = 1;

        foreach (var row in table.Elements<TableRow>().Skip(1))
        {
            var cells = row.Elements<TableCell>().ToList();
            if (cells.Count < 5) continue;

            var name = GetCellText(cells[1]);
            if (string.IsNullOrWhiteSpace(name)) continue;

            result.Add(new SupervisorResponse
            {
                FullName     = name,
                Phone        = NullIfEmpty(GetCellText(cells[2])),
                Email        = NullIfEmpty(GetCellText(cells[3])),
                Title        = NullIfEmpty(GetCellText(cells[4])),
                IsPrimary    = order == 1,
                DisplayOrder = order++,
            });
        }

        return result;
    }

    private static List<StudentResponse> ParseStudentsTable(Table table)
    {
        var result = new List<StudentResponse>();
        int order  = 1;

        foreach (var row in table.Elements<TableRow>().Skip(1))
        {
            var cells = row.Elements<TableCell>().ToList();
            if (cells.Count < 6) continue;

            var name = GetCellText(cells[1]);
            if (string.IsNullOrWhiteSpace(name)) continue;

            result.Add(new StudentResponse
            {
                FullName     = name,
                StudentCode  = NullIfEmpty(GetCellText(cells[2])),
                Phone        = NullIfEmpty(GetCellText(cells[3])),
                Email        = NullIfEmpty(GetCellText(cells[4])),
                RoleInGroup  = NullIfEmpty(GetCellText(cells[5])),
                DisplayOrder = order++,
            });
        }

        return result;
    }

    private static Dictionary<string, int> BuildImageSizeMap(MainDocumentPart mainPart)
    {
        var map = new Dictionary<string, int>();
        foreach (var imgPart in mainPart.ImageParts)
        {
            var rId = mainPart.GetIdOfPart(imgPart);
            using var ms  = new MemoryStream();
            using var src = imgPart.GetStream();
            src.CopyTo(ms);
            map[rId] = (int)ms.Length;
        }
        return map;
    }

    private static bool IsChecked(WpDrawing drawing, Dictionary<string, int> imageSizes)
    {
        var blip = drawing.Descendants<ABlip>().FirstOrDefault();
        if (blip?.Embed?.Value is not { } rId) return false;
        return imageSizes.TryGetValue(rId, out var size) && size >= CheckedThresholdBytes;
    }

    private static string GetText(OpenXmlElement el) =>
        string.Concat(el.Descendants<Text>().Select(t => t.Text));

    private static string GetCellText(TableCell cell) =>
        GetText(cell).Trim();

    private static string AfterColon(string text)
    {
        var idx = text.IndexOf(':');
        return idx >= 0 ? text[(idx + 1)..].Trim() : text.Trim();
    }

    private static string? NullIfEmpty(string? s) =>
        string.IsNullOrWhiteSpace(s) ? null : s;

    private static bool IsPlaceholder(string s)
    {
        var stripped = s.Replace(".", "").Replace("…", "").Replace("/", "")
                        .Replace(" ", "").Replace("\t", "");
        return string.IsNullOrEmpty(stripped);
    }

    private static DateOnly? TryParseDate(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw) || IsPlaceholder(raw)) return null;

        var cleaned = raw.Trim().Replace("…", "").Replace(" ", "");

        string[] formats = ["d/M/yyyy", "dd/MM/yyyy", "M/yyyy", "MM/yyyy"];
        foreach (var fmt in formats)
        {
            if (DateOnly.TryParseExact(cleaned, fmt, CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out var date))
                return date;
        }
        return null;
    }

    [GeneratedRegex(@"Class:\s*(.+?)\s{3,}Duration time:", RegexOptions.IgnoreCase)]
    private static partial Regex ClassNameRegex();

    [GeneratedRegex(@"from\s+([\d/\.…]+)\s+To\s+([\d/\.…]+)", RegexOptions.IgnoreCase)]
    private static partial Regex DurationRegex();

    [GeneratedRegex(@"Profession:\s*(.+?)\s{3,}Specialty", RegexOptions.IgnoreCase)]
    private static partial Regex ProfessionRegex();
}
