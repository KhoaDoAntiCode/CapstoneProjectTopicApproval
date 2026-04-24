# Feature Layer Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Implement Auth, Semester, Project, and Review features as a complete 3-tier (Controller→Service→Repository) stack on top of the existing infrastructure layer.

**Architecture:** Each feature follows the same pattern: DTO → Repository Interface → Repository Implementation → Service Interface → Service Implementation → Controller. Repos and services are registered in `ServiceCollectionExtensions.cs`. All controller responses use `ApiResponse<T>`. All errors throw `AppException` subclasses caught by `ExceptionHandlingMiddleware`.

**Tech Stack:** .NET 9, ASP.NET Core, EF Core 8 (Npgsql), BCrypt.Net-Next, System.IdentityModel.Tokens.Jwt, DocumentFormat.OpenXml (DOCX parsing — stub only, needs form structure info)

**Note on DOCX parsing:** `DocxParserService` is implemented as a stub that throws `NotImplementedException`. A follow-up session will implement the real parser once the form structure is known.

**Note on tests:** No test project exists in this repo. Skip all test steps.

---

## File Map

### Auth
| Action | Path |
|---|---|
| Create | `DTOs/Requests/RegisterRequest.cs` |
| Create | `DTOs/Requests/LoginRequest.cs` |
| Create | `DTOs/Responses/AuthResponse.cs` |
| Create | `Services/Interfaces/IAuthService.cs` |
| Create | `Services/Implementations/AuthService.cs` |
| Create | `Controllers/AuthController.cs` |
| Modify | `Extensions/ServiceCollectionExtensions.cs` |

### Semester
| Action | Path |
|---|---|
| Create | `DTOs/Responses/SemesterResponse.cs` |
| Create | `Repositories/Interfaces/ISemesterRepository.cs` |
| Create | `Repositories/Implementations/SemesterRepository.cs` |
| Create | `Controllers/SemesterController.cs` |
| Modify | `Extensions/ServiceCollectionExtensions.cs` |

### Project
| Action | Path |
|---|---|
| Create | `DTOs/Requests/SubmitProjectRequest.cs` |
| Create | `DTOs/Responses/ProjectResponse.cs` |
| Create | `DTOs/Responses/ProjectListItemResponse.cs` |
| Create | `DTOs/Responses/DocxPreviewResponse.cs` |
| Create | `Services/Interfaces/IDocxParserService.cs` |
| Create | `Services/Implementations/DocxParserService.cs` (stub) |
| Create | `Repositories/Interfaces/ICapstoneProjectRepository.cs` |
| Create | `Repositories/Implementations/CapstoneProjectRepository.cs` |
| Create | `Services/Interfaces/IProjectService.cs` |
| Create | `Services/Implementations/ProjectService.cs` |
| Create | `Controllers/ProjectController.cs` |
| Modify | `Extensions/ServiceCollectionExtensions.cs` |

### Review
| Action | Path |
|---|---|
| Create | `DTOs/Requests/ReviewRequest.cs` |
| Create | `DTOs/Responses/ReviewResponse.cs` |
| Create | `Repositories/Interfaces/IProjectReviewRepository.cs` |
| Create | `Repositories/Implementations/ProjectReviewRepository.cs` |
| Create | `Services/Interfaces/IReviewService.cs` |
| Create | `Services/Implementations/ReviewService.cs` |
| Create | `Controllers/ReviewController.cs` |
| Modify | `Extensions/ServiceCollectionExtensions.cs` |

---

## Task 1: Auth DTOs

**Files:**
- Create: `CapstoneRegistration.API/DTOs/Requests/RegisterRequest.cs`
- Create: `CapstoneRegistration.API/DTOs/Requests/LoginRequest.cs`
- Create: `CapstoneRegistration.API/DTOs/Responses/AuthResponse.cs`

- [ ] Create `RegisterRequest.cs`:

```csharp
using System.ComponentModel.DataAnnotations;

namespace CapstoneRegistration.API.DTOs.Requests;

public class RegisterRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;

    [Required]
    [MinLength(8)]
    public string Password { get; set; } = null!;

    [Required]
    [MaxLength(255)]
    public string FullName { get; set; } = null!;
}
```

- [ ] Create `LoginRequest.cs`:

```csharp
using System.ComponentModel.DataAnnotations;

namespace CapstoneRegistration.API.DTOs.Requests;

public class LoginRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;

    [Required]
    public string Password { get; set; } = null!;
}
```

- [ ] Create `AuthResponse.cs`:

```csharp
namespace CapstoneRegistration.API.DTOs.Responses;

public class AuthResponse
{
    public string Token { get; set; } = null!;
    public Guid UserId { get; set; }
    public string Email { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string Role { get; set; } = null!;
    public DateTime ExpiresAt { get; set; }
}
```

- [ ] Commit: `feat: add auth DTOs`

---

## Task 2: IAuthService + AuthService

**Files:**
- Create: `CapstoneRegistration.API/Services/Interfaces/IAuthService.cs`
- Create: `CapstoneRegistration.API/Services/Implementations/AuthService.cs`

- [ ] Create `IAuthService.cs`:

```csharp
using CapstoneRegistration.API.DTOs.Requests;
using CapstoneRegistration.API.DTOs.Responses;

namespace CapstoneRegistration.API.Services.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default);
    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default);
}
```

- [ ] Create `AuthService.cs`:

```csharp
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using CapstoneRegistration.API.Data;
using CapstoneRegistration.API.DTOs.Requests;
using CapstoneRegistration.API.DTOs.Responses;
using CapstoneRegistration.API.Exceptions;
using CapstoneRegistration.API.Models;
using CapstoneRegistration.API.Services.Interfaces;

namespace CapstoneRegistration.API.Services.Implementations;

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _db;
    private readonly IConfiguration _config;

    public AuthService(ApplicationDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        var allowedDomain = _config["AllowedEmailDomain"] ?? "fpt.edu.vn";
        var domain = request.Email.Split('@').LastOrDefault();
        if (!string.Equals(domain, allowedDomain, StringComparison.OrdinalIgnoreCase))
            throw new BadRequestException($"Only @{allowedDomain} email addresses are allowed.");

        var exists = await _db.Users.AnyAsync(u => u.Email == request.Email.ToLower(), ct);
        if (exists)
            throw new BadRequestException("An account with this email already exists.");

        var user = new User
        {
            Email        = request.Email.ToLower(),
            FullName     = request.FullName,
            Role         = "Lecturer",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            IsEmailVerified = false
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync(ct);

        return BuildAuthResponse(user);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email.ToLower(), ct);

        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedException("Invalid email or password.");

        return BuildAuthResponse(user);
    }

    private AuthResponse BuildAuthResponse(User user)
    {
        var jwtSection  = _config.GetSection("Jwt");
        var key         = jwtSection["Key"]!;
        var issuer      = jwtSection["Issuer"]!;
        var audience    = jwtSection["Audience"]!;
        var expiryMins  = int.Parse(jwtSection["ExpiryMinutes"] ?? "60");
        var expiresAt   = DateTime.UtcNow.AddMinutes(expiryMins);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub,   user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Role,               user.Role),
            new Claim(JwtRegisteredClaimNames.Jti,   Guid.NewGuid().ToString())
        };

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var token = new JwtSecurityToken(
            issuer:             issuer,
            audience:           audience,
            claims:             claims,
            expires:            expiresAt,
            signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256)
        );

        return new AuthResponse
        {
            Token     = new JwtSecurityTokenHandler().WriteToken(token),
            UserId    = user.Id,
            Email     = user.Email,
            FullName  = user.FullName,
            Role      = user.Role,
            ExpiresAt = expiresAt
        };
    }
}
```

- [ ] Commit: `feat: add AuthService with @fpt.edu.vn validation and JWT`

---

## Task 3: AuthController

**Files:**
- Create: `CapstoneRegistration.API/Controllers/AuthController.cs`
- Modify: `CapstoneRegistration.API/Extensions/ServiceCollectionExtensions.cs` — register IAuthService

- [ ] Create `AuthController.cs`:

```csharp
using Microsoft.AspNetCore.Mvc;
using CapstoneRegistration.API.Common;
using CapstoneRegistration.API.DTOs.Requests;
using CapstoneRegistration.API.Services.Interfaces;

namespace CapstoneRegistration.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService) => _authService = authService;

    /// <summary>Register a new lecturer account. Email must be @fpt.edu.vn.</summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register(
        [FromBody] RegisterRequest request,
        CancellationToken ct)
    {
        var result = await _authService.RegisterAsync(request, ct);
        return Ok(ApiResponse<object>.Ok(result, "Registration successful."));
    }

    /// <summary>Login and receive a JWT token.</summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request,
        CancellationToken ct)
    {
        var result = await _authService.LoginAsync(request, ct);
        return Ok(ApiResponse<object>.Ok(result));
    }
}
```

- [ ] In `ServiceCollectionExtensions.cs`, under the Domain Services section add:
  `services.AddScoped<IAuthService, AuthService>();`

- [ ] Commit: `feat: add AuthController (POST /api/auth/register, /api/auth/login)`

---

## Task 4: Semester repository + controller

**Files:**
- Create: `CapstoneRegistration.API/DTOs/Responses/SemesterResponse.cs`
- Create: `CapstoneRegistration.API/Repositories/Interfaces/ISemesterRepository.cs`
- Create: `CapstoneRegistration.API/Repositories/Implementations/SemesterRepository.cs`
- Create: `CapstoneRegistration.API/Controllers/SemesterController.cs`
- Modify: `CapstoneRegistration.API/Extensions/ServiceCollectionExtensions.cs`

- [ ] Create `SemesterResponse.cs`:

```csharp
namespace CapstoneRegistration.API.DTOs.Responses;

public class SemesterResponse
{
    public string Id { get; set; } = null!;       // e.g. "SU26"
    public string Name { get; set; } = null!;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public bool IsActive { get; set; }
}
```

- [ ] Create `ISemesterRepository.cs`:

```csharp
using CapstoneRegistration.API.Models;

namespace CapstoneRegistration.API.Repositories.Interfaces;

public interface ISemesterRepository
{
    Task<IReadOnlyList<Semester>> GetAllAsync(CancellationToken ct = default);
    Task<Semester?> GetByIdAsync(string id, CancellationToken ct = default);
    /// <summary>Find a semester whose date range contains the given date.</summary>
    Task<Semester?> DetectByDateAsync(DateOnly date, CancellationToken ct = default);
}
```

- [ ] Create `SemesterRepository.cs`:

```csharp
using Microsoft.EntityFrameworkCore;
using CapstoneRegistration.API.Data;
using CapstoneRegistration.API.Models;
using CapstoneRegistration.API.Repositories.Interfaces;

namespace CapstoneRegistration.API.Repositories.Implementations;

public class SemesterRepository : ISemesterRepository
{
    private readonly ApplicationDbContext _db;
    public SemesterRepository(ApplicationDbContext db) => _db = db;

    public async Task<IReadOnlyList<Semester>> GetAllAsync(CancellationToken ct = default) =>
        await _db.Semesters.OrderByDescending(s => s.StartDate).ToListAsync(ct);

    public async Task<Semester?> GetByIdAsync(string id, CancellationToken ct = default) =>
        await _db.Semesters.FindAsync([id], ct);

    public async Task<Semester?> DetectByDateAsync(DateOnly date, CancellationToken ct = default) =>
        await _db.Semesters
            .Where(s => s.StartDate <= date && s.EndDate >= date)
            .FirstOrDefaultAsync(ct);
}
```

- [ ] Create `SemesterController.cs`:

```csharp
using Microsoft.AspNetCore.Mvc;
using CapstoneRegistration.API.Common;
using CapstoneRegistration.API.DTOs.Responses;
using CapstoneRegistration.API.Repositories.Interfaces;

namespace CapstoneRegistration.API.Controllers;

[ApiController]
[Route("api/semesters")]
public class SemesterController : ControllerBase
{
    private readonly ISemesterRepository _repo;
    public SemesterController(ISemesterRepository repo) => _repo = repo;

    /// <summary>List all semesters ordered by start date descending.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var semesters = await _repo.GetAllAsync(ct);
        var response = semesters.Select(s => new SemesterResponse
        {
            Id        = s.Id,
            Name      = s.Name,
            StartDate = s.StartDate,
            EndDate   = s.EndDate,
            IsActive  = s.IsActive
        }).ToList();

        return Ok(ApiResponse<object>.Ok(response));
    }
}
```

- [ ] In `ServiceCollectionExtensions.cs` Domain Repositories section add:
  `services.AddScoped<ISemesterRepository, SemesterRepository>();`

- [ ] Commit: `feat: add Semester repository and GET /api/semesters`

---

## Task 5: Project DTOs + DOCX parser stub

**Files:**
- Create: `CapstoneRegistration.API/DTOs/Requests/SubmitProjectRequest.cs`
- Create: `CapstoneRegistration.API/DTOs/Responses/ProjectResponse.cs`
- Create: `CapstoneRegistration.API/DTOs/Responses/ProjectListItemResponse.cs`
- Create: `CapstoneRegistration.API/DTOs/Responses/DocxPreviewResponse.cs`
- Create: `CapstoneRegistration.API/Services/Interfaces/IDocxParserService.cs`
- Create: `CapstoneRegistration.API/Services/Implementations/DocxParserService.cs`

- [ ] Create `SubmitProjectRequest.cs`:

```csharp
using System.ComponentModel.DataAnnotations;

namespace CapstoneRegistration.API.DTOs.Requests;

/// <summary>
/// Payload sent by the lecturer after reviewing the parsed DOCX preview.
/// All fields correspond 1-to-1 with CapstoneProject columns.
/// SemesterId may be auto-detected from duration dates or manually selected.
/// </summary>
public class SubmitProjectRequest
{
    [Required]
    public string SemesterId { get; set; } = null!;

    [Required]
    [MaxLength(500)]
    public string EnglishName { get; set; } = null!;

    [Required]
    [MaxLength(500)]
    public string VietnameseName { get; set; } = null!;

    [MaxLength(20)]
    public string? Abbreviation { get; set; }

    public bool IsResearchProject { get; set; }
    public bool IsEnterpriseProject { get; set; }

    public string? Context { get; set; }
    public string? ProposedSolutions { get; set; }
    public string? FunctionalRequirements { get; set; }
    public string? NonFunctionalRequirements { get; set; }
    public string? TheoryAndPractice { get; set; }
    public string? Products { get; set; }
    public string? ProposedTasks { get; set; }

    [MaxLength(20)]
    public string? ClassName { get; set; }

    public DateOnly? DurationFrom { get; set; }
    public DateOnly? DurationTo { get; set; }

    [MaxLength(100)]
    public string? Profession { get; set; }

    [MaxLength(10)]
    public string? Specialty { get; set; } // ES | IS | JS

    [MaxLength(20)]
    public string? RegisterKind { get; set; } // Lecturer | Students

    public List<SupervisorRequest> Supervisors { get; set; } = [];
    public List<StudentRequest> Students { get; set; } = [];
}

public class SupervisorRequest
{
    [Required]
    [MaxLength(255)]
    public string FullName { get; set; } = null!;
    [MaxLength(20)]  public string? Phone { get; set; }
    [MaxLength(255)] public string? Email { get; set; }
    [MaxLength(100)] public string? Title { get; set; }
    public bool IsPrimary { get; set; }
    public int DisplayOrder { get; set; }
}

public class StudentRequest
{
    [Required]
    [MaxLength(255)]
    public string FullName { get; set; } = null!;
    [MaxLength(20)]  public string? StudentCode { get; set; }
    [MaxLength(20)]  public string? Phone { get; set; }
    [MaxLength(255)] public string? Email { get; set; }
    [MaxLength(10)]  public string? RoleInGroup { get; set; } // Leader | Member
    public int DisplayOrder { get; set; }
}
```

- [ ] Create `ProjectResponse.cs`:

```csharp
namespace CapstoneRegistration.API.DTOs.Responses;

public class ProjectResponse
{
    public Guid Id { get; set; }
    public string ProjectCode { get; set; } = null!;
    public string SemesterId { get; set; } = null!;
    public string EnglishName { get; set; } = null!;
    public string VietnameseName { get; set; } = null!;
    public string? Abbreviation { get; set; }
    public bool IsResearchProject { get; set; }
    public bool IsEnterpriseProject { get; set; }
    public string? Context { get; set; }
    public string? ProposedSolutions { get; set; }
    public string? FunctionalRequirements { get; set; }
    public string? NonFunctionalRequirements { get; set; }
    public string? TheoryAndPractice { get; set; }
    public string? Products { get; set; }
    public string? ProposedTasks { get; set; }
    public string? ClassName { get; set; }
    public DateOnly? DurationFrom { get; set; }
    public DateOnly? DurationTo { get; set; }
    public string? Profession { get; set; }
    public string? Specialty { get; set; }
    public string? RegisterKind { get; set; }
    public string Status { get; set; } = null!;
    public Guid CreatedById { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<SupervisorResponse> Supervisors { get; set; } = [];
    public List<StudentResponse> Students { get; set; } = [];
    public List<ReviewResponse> Reviews { get; set; } = [];
}

public class SupervisorResponse
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = null!;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Title { get; set; }
    public bool IsPrimary { get; set; }
    public int DisplayOrder { get; set; }
}

public class StudentResponse
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = null!;
    public string? StudentCode { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? RoleInGroup { get; set; }
    public int DisplayOrder { get; set; }
}
```

- [ ] Create `ProjectListItemResponse.cs`:

```csharp
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
```

- [ ] Create `DocxPreviewResponse.cs`:

```csharp
namespace CapstoneRegistration.API.DTOs.Responses;

/// <summary>
/// Data extracted from an uploaded DOCX file.
/// Returned to the lecturer for review before final submission.
/// Nothing is persisted at this stage.
/// </summary>
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
```

- [ ] Create `IDocxParserService.cs`:

```csharp
using CapstoneRegistration.API.DTOs.Responses;

namespace CapstoneRegistration.API.Services.Interfaces;

public interface IDocxParserService
{
    /// <summary>
    /// Parse a DOCX stream and extract all project fields.
    /// The detected semester is resolved against the database using DurationFrom/To dates.
    /// Returns a preview DTO — nothing is persisted.
    /// </summary>
    Task<DocxPreviewResponse> ParseAsync(Stream docxStream, CancellationToken ct = default);
}
```

- [ ] Create `DocxParserService.cs` (stub — needs real DOCX form structure):

```csharp
using CapstoneRegistration.API.DTOs.Responses;
using CapstoneRegistration.API.Repositories.Interfaces;
using CapstoneRegistration.API.Services.Interfaces;

namespace CapstoneRegistration.API.Services.Implementations;

/// <summary>
/// STUB — Not yet implemented.
///
/// To implement: inspect the actual DOCX registration form to determine:
/// 1. Are fields in a Word table? If so, which row/column indices?
/// 2. Are fields in content controls (SDT)? If so, what are the tags/aliases?
/// 3. Are fields identified by bookmark names?
/// 4. Is the structure consistent across all submitted DOCX versions?
///
/// Add DocumentFormat.OpenXml package once the structure is confirmed.
/// </summary>
public class DocxParserService : IDocxParserService
{
    private readonly ISemesterRepository _semesterRepo;

    public DocxParserService(ISemesterRepository semesterRepo)
    {
        _semesterRepo = semesterRepo;
    }

    public Task<DocxPreviewResponse> ParseAsync(Stream docxStream, CancellationToken ct = default)
    {
        throw new NotImplementedException(
            "DOCX parser not yet implemented. " +
            "Provide a sample DOCX registration form so the table/control structure can be mapped.");
    }
}
```

- [ ] In `ServiceCollectionExtensions.cs` Domain Services section add:
  `services.AddScoped<IDocxParserService, DocxParserService>();`

- [ ] Commit: `feat: add project DTOs and DOCX parser stub`

---

## Task 6: Project repository

**Files:**
- Create: `CapstoneRegistration.API/Repositories/Interfaces/ICapstoneProjectRepository.cs`
- Create: `CapstoneRegistration.API/Repositories/Implementations/CapstoneProjectRepository.cs`

- [ ] Create `ICapstoneProjectRepository.cs`:

```csharp
using CapstoneRegistration.API.Common;
using CapstoneRegistration.API.Models;

namespace CapstoneRegistration.API.Repositories.Interfaces;

public interface ICapstoneProjectRepository
{
    Task<CapstoneProject?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default);
    Task<PagedResult<CapstoneProject>> GetPagedAsync(
        int page, int pageSize,
        string? semesterId = null,
        string? status = null,
        string? search = null,
        CancellationToken ct = default);
    Task<string> GenerateProjectCodeAsync(string semesterId, CancellationToken ct = default);
    Task<CapstoneProject> AddAsync(CapstoneProject project, CancellationToken ct = default);
}
```

- [ ] Create `CapstoneProjectRepository.cs`:

```csharp
using Microsoft.EntityFrameworkCore;
using CapstoneRegistration.API.Common;
using CapstoneRegistration.API.Data;
using CapstoneRegistration.API.Models;
using CapstoneRegistration.API.Repositories.Interfaces;

namespace CapstoneRegistration.API.Repositories.Implementations;

public class CapstoneProjectRepository : ICapstoneProjectRepository
{
    private readonly ApplicationDbContext _db;
    public CapstoneProjectRepository(ApplicationDbContext db) => _db = db;

    public async Task<CapstoneProject?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default) =>
        await _db.CapstoneProjects
            .Include(p => p.Supervisors.OrderBy(s => s.DisplayOrder))
            .Include(p => p.Students.OrderBy(s => s.DisplayOrder))
            .Include(p => p.ProjectReviews.OrderBy(r => r.ReviewedAt))
                .ThenInclude(r => r.ReviewedBy)
            .Include(p => p.CreatedBy)
            .FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<PagedResult<CapstoneProject>> GetPagedAsync(
        int page, int pageSize,
        string? semesterId = null,
        string? status = null,
        string? search = null,
        CancellationToken ct = default)
    {
        var query = _db.CapstoneProjects
            .Include(p => p.CreatedBy)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(semesterId))
            query = query.Where(p => p.SemesterId == semesterId);

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(p => p.Status == status);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(p =>
                p.EnglishName.Contains(search) ||
                p.VietnameseName.Contains(search) ||
                p.ProjectCode.Contains(search));

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return PagedResult<CapstoneProject>.Create(items, total, page, pageSize);
    }

    public async Task<string> GenerateProjectCodeAsync(string semesterId, CancellationToken ct = default)
    {
        // Code format: {semesterId}{specialty_prefix}{sequence} e.g. "SU26SE001"
        // For generality, use a simple numeric sequence per semester.
        var count = await _db.CapstoneProjects
            .CountAsync(p => p.SemesterId == semesterId, ct);
        return $"{semesterId}{(count + 1):D3}";
    }

    public async Task<CapstoneProject> AddAsync(CapstoneProject project, CancellationToken ct = default)
    {
        _db.CapstoneProjects.Add(project);
        await _db.SaveChangesAsync(ct);
        return project;
    }
}
```

- [ ] In `ServiceCollectionExtensions.cs` Domain Repositories section add:
  `services.AddScoped<ICapstoneProjectRepository, CapstoneProjectRepository>();`

- [ ] Commit: `feat: add CapstoneProjectRepository`

---

## Task 7: ProjectService

**Files:**
- Create: `CapstoneRegistration.API/Services/Interfaces/IProjectService.cs`
- Create: `CapstoneRegistration.API/Services/Implementations/ProjectService.cs`

- [ ] Create `IProjectService.cs`:

```csharp
using CapstoneRegistration.API.Common;
using CapstoneRegistration.API.DTOs.Requests;
using CapstoneRegistration.API.DTOs.Responses;

namespace CapstoneRegistration.API.Services.Interfaces;

public interface IProjectService
{
    Task<DocxPreviewResponse> ParseDocxAsync(Stream docxStream, CancellationToken ct = default);
    Task<ProjectResponse> SubmitAsync(Guid createdById, SubmitProjectRequest request, CancellationToken ct = default);
    Task<PagedResult<ProjectListItemResponse>> GetPagedAsync(
        int page, int pageSize,
        string? semesterId, string? status, string? search,
        CancellationToken ct = default);
    Task<ProjectResponse> GetByIdAsync(Guid id, CancellationToken ct = default);
}
```

- [ ] Create `ProjectService.cs`:

```csharp
using CapstoneRegistration.API.Common;
using CapstoneRegistration.API.DTOs.Requests;
using CapstoneRegistration.API.DTOs.Responses;
using CapstoneRegistration.API.Exceptions;
using CapstoneRegistration.API.Models;
using CapstoneRegistration.API.Repositories.Interfaces;
using CapstoneRegistration.API.Services.Interfaces;

namespace CapstoneRegistration.API.Services.Implementations;

public class ProjectService : IProjectService
{
    private readonly ICapstoneProjectRepository _projectRepo;
    private readonly ISemesterRepository _semesterRepo;
    private readonly IDocxParserService _docxParser;

    public ProjectService(
        ICapstoneProjectRepository projectRepo,
        ISemesterRepository semesterRepo,
        IDocxParserService docxParser)
    {
        _projectRepo  = projectRepo;
        _semesterRepo = semesterRepo;
        _docxParser   = docxParser;
    }

    public Task<DocxPreviewResponse> ParseDocxAsync(Stream docxStream, CancellationToken ct = default) =>
        _docxParser.ParseAsync(docxStream, ct);

    public async Task<ProjectResponse> SubmitAsync(
        Guid createdById,
        SubmitProjectRequest request,
        CancellationToken ct = default)
    {
        var semester = await _semesterRepo.GetByIdAsync(request.SemesterId, ct)
            ?? throw new NotFoundException("Semester", request.SemesterId);

        var projectCode = await _projectRepo.GenerateProjectCodeAsync(semester.Id, ct);

        var project = new CapstoneProject
        {
            ProjectCode             = projectCode,
            SemesterId              = semester.Id,
            CreatedById             = createdById,
            EnglishName             = request.EnglishName,
            VietnameseName          = request.VietnameseName,
            Abbreviation            = request.Abbreviation,
            IsResearchProject       = request.IsResearchProject,
            IsEnterpriseProject     = request.IsEnterpriseProject,
            Context                 = request.Context,
            ProposedSolutions       = request.ProposedSolutions,
            FunctionalRequirements  = request.FunctionalRequirements,
            NonFunctionalRequirements = request.NonFunctionalRequirements,
            TheoryAndPractice       = request.TheoryAndPractice,
            Products                = request.Products,
            ProposedTasks           = request.ProposedTasks,
            ClassName               = request.ClassName,
            DurationFrom            = request.DurationFrom,
            DurationTo              = request.DurationTo,
            Profession              = request.Profession,
            Specialty               = request.Specialty,
            RegisterKind            = request.RegisterKind,
            Status                  = "Pending",
            Supervisors = request.Supervisors.Select((s, i) => new ProjectSupervisor
            {
                FullName     = s.FullName,
                Phone        = s.Phone,
                Email        = s.Email,
                Title        = s.Title,
                IsPrimary    = s.IsPrimary,
                DisplayOrder = s.DisplayOrder > 0 ? s.DisplayOrder : i
            }).ToList(),
            Students = request.Students.Select((s, i) => new ProjectStudent
            {
                FullName     = s.FullName,
                StudentCode  = s.StudentCode,
                Phone        = s.Phone,
                Email        = s.Email,
                RoleInGroup  = s.RoleInGroup,
                DisplayOrder = s.DisplayOrder > 0 ? s.DisplayOrder : i
            }).ToList()
        };

        await _projectRepo.AddAsync(project, ct);

        // Reload with all navigation properties for the response
        var saved = await _projectRepo.GetByIdWithDetailsAsync(project.Id, ct)
            ?? throw new NotFoundException("CapstoneProject", project.Id);

        return MapToResponse(saved);
    }

    public async Task<PagedResult<ProjectListItemResponse>> GetPagedAsync(
        int page, int pageSize,
        string? semesterId, string? status, string? search,
        CancellationToken ct = default)
    {
        var paged = await _projectRepo.GetPagedAsync(page, pageSize, semesterId, status, search, ct);
        var items = paged.Items.Select(p => new ProjectListItemResponse
        {
            Id            = p.Id,
            ProjectCode   = p.ProjectCode,
            SemesterId    = p.SemesterId,
            EnglishName   = p.EnglishName,
            VietnameseName = p.VietnameseName,
            Specialty     = p.Specialty,
            Status        = p.Status,
            CreatedByName = p.CreatedBy.FullName,
            CreatedAt     = p.CreatedAt
        }).ToList();

        return PagedResult<ProjectListItemResponse>.Create(items, paged.TotalCount, page, pageSize);
    }

    public async Task<ProjectResponse> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var project = await _projectRepo.GetByIdWithDetailsAsync(id, ct)
            ?? throw new NotFoundException("CapstoneProject", id);
        return MapToResponse(project);
    }

    private static ProjectResponse MapToResponse(CapstoneProject p) => new()
    {
        Id                        = p.Id,
        ProjectCode               = p.ProjectCode,
        SemesterId                = p.SemesterId,
        EnglishName               = p.EnglishName,
        VietnameseName            = p.VietnameseName,
        Abbreviation              = p.Abbreviation,
        IsResearchProject         = p.IsResearchProject,
        IsEnterpriseProject       = p.IsEnterpriseProject,
        Context                   = p.Context,
        ProposedSolutions         = p.ProposedSolutions,
        FunctionalRequirements    = p.FunctionalRequirements,
        NonFunctionalRequirements = p.NonFunctionalRequirements,
        TheoryAndPractice         = p.TheoryAndPractice,
        Products                  = p.Products,
        ProposedTasks             = p.ProposedTasks,
        ClassName                 = p.ClassName,
        DurationFrom              = p.DurationFrom,
        DurationTo                = p.DurationTo,
        Profession                = p.Profession,
        Specialty                 = p.Specialty,
        RegisterKind              = p.RegisterKind,
        Status                    = p.Status,
        CreatedById               = p.CreatedById,
        CreatedAt                 = p.CreatedAt,
        UpdatedAt                 = p.UpdatedAt,
        Supervisors = p.Supervisors.Select(s => new SupervisorResponse
        {
            Id           = s.Id,
            FullName     = s.FullName,
            Phone        = s.Phone,
            Email        = s.Email,
            Title        = s.Title,
            IsPrimary    = s.IsPrimary,
            DisplayOrder = s.DisplayOrder
        }).ToList(),
        Students = p.Students.Select(s => new StudentResponse
        {
            Id           = s.Id,
            FullName     = s.FullName,
            StudentCode  = s.StudentCode,
            Phone        = s.Phone,
            Email        = s.Email,
            RoleInGroup  = s.RoleInGroup,
            DisplayOrder = s.DisplayOrder
        }).ToList(),
        Reviews = p.ProjectReviews.Select(r => new ReviewResponse
        {
            Id             = r.Id,
            ProjectId      = r.ProjectId,
            ReviewedById   = r.ReviewedById,
            ReviewedByName = r.ReviewedBy.FullName,
            Decision       = r.Decision,
            Comment        = r.Comment,
            ReviewedAt     = r.ReviewedAt
        }).ToList()
    };
}
```

- [ ] In `ServiceCollectionExtensions.cs` Domain Services section add:
  `services.AddScoped<IProjectService, ProjectService>();`

- [ ] Commit: `feat: add ProjectService`

---

## Task 8: ProjectController

**Files:**
- Create: `CapstoneRegistration.API/Controllers/ProjectController.cs`

- [ ] Create `ProjectController.cs`:

```csharp
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CapstoneRegistration.API.Common;
using CapstoneRegistration.API.DTOs.Requests;
using CapstoneRegistration.API.Exceptions;
using CapstoneRegistration.API.Services.Interfaces;

namespace CapstoneRegistration.API.Controllers;

[ApiController]
[Route("api/projects")]
[Authorize]
public class ProjectController : ControllerBase
{
    private readonly IProjectService _projectService;

    public ProjectController(IProjectService projectService) =>
        _projectService = projectService;

    /// <summary>
    /// Upload a DOCX registration form and receive a parsed preview.
    /// Nothing is saved at this stage — the lecturer reviews and edits before submitting.
    /// </summary>
    [HttpPost("parse")]
    public async Task<IActionResult> ParseDocx(
        IFormFile file,
        CancellationToken ct)
    {
        if (file is null || file.Length == 0)
            throw new BadRequestException("No file uploaded.");

        if (!file.FileName.EndsWith(".docx", StringComparison.OrdinalIgnoreCase))
            throw new BadRequestException("Only .docx files are accepted.");

        await using var stream = file.OpenReadStream();
        var preview = await _projectService.ParseDocxAsync(stream, ct);
        return Ok(ApiResponse<object>.Ok(preview));
    }

    /// <summary>
    /// Submit a project (manual or from DOCX preview). Saved with status = Pending.
    /// Requires authentication — the caller becomes the project creator.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Submit(
        [FromBody] SubmitProjectRequest request,
        CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        var result = await _projectService.SubmitAsync(userId, request, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id },
            ApiResponse<object>.Ok(result, "Project submitted successfully."));
    }

    /// <summary>
    /// List projects with optional filters. Supports pagination.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? semesterId = null,
        [FromQuery] string? status = null,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
    {
        if (page < 1) page = 1;
        if (pageSize is < 1 or > 100) pageSize = 20;

        var result = await _projectService.GetPagedAsync(page, pageSize, semesterId, status, search, ct);
        return Ok(ApiResponse<object>.Ok(result));
    }

    /// <summary>Get full project details including supervisors, students, and review history.</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _projectService.GetByIdAsync(id, ct);
        return Ok(ApiResponse<object>.Ok(result));
    }

    private Guid GetCurrentUserId()
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier)
               ?? User.FindFirstValue("sub");
        if (sub is null || !Guid.TryParse(sub, out var userId))
            throw new UnauthorizedException("Cannot determine current user.");
        return userId;
    }
}
```

- [ ] Commit: `feat: add ProjectController (parse/submit/list/get)`

---

## Task 9: Review feature (Repository + Service + Controller)

**Files:**
- Create: `CapstoneRegistration.API/DTOs/Requests/ReviewRequest.cs`
- Create: `CapstoneRegistration.API/DTOs/Responses/ReviewResponse.cs`
- Create: `CapstoneRegistration.API/Repositories/Interfaces/IProjectReviewRepository.cs`
- Create: `CapstoneRegistration.API/Repositories/Implementations/ProjectReviewRepository.cs`
- Create: `CapstoneRegistration.API/Services/Interfaces/IReviewService.cs`
- Create: `CapstoneRegistration.API/Services/Implementations/ReviewService.cs`
- Create: `CapstoneRegistration.API/Controllers/ReviewController.cs`
- Modify: `CapstoneRegistration.API/Extensions/ServiceCollectionExtensions.cs`

- [ ] Create `ReviewRequest.cs`:

```csharp
using System.ComponentModel.DataAnnotations;

namespace CapstoneRegistration.API.DTOs.Requests;

public class ReviewRequest
{
    [Required]
    public string Decision { get; set; } = null!; // Accepted | Denied

    public string? Comment { get; set; }
}
```

- [ ] Create `ReviewResponse.cs`:

```csharp
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
```

- [ ] Create `IProjectReviewRepository.cs`:

```csharp
using CapstoneRegistration.API.Models;

namespace CapstoneRegistration.API.Repositories.Interfaces;

public interface IProjectReviewRepository
{
    Task<IReadOnlyList<ProjectReview>> GetByProjectIdAsync(Guid projectId, CancellationToken ct = default);
    Task<ProjectReview> AddAsync(ProjectReview review, CancellationToken ct = default);
}
```

- [ ] Create `ProjectReviewRepository.cs`:

```csharp
using Microsoft.EntityFrameworkCore;
using CapstoneRegistration.API.Data;
using CapstoneRegistration.API.Models;
using CapstoneRegistration.API.Repositories.Interfaces;

namespace CapstoneRegistration.API.Repositories.Implementations;

public class ProjectReviewRepository : IProjectReviewRepository
{
    private readonly ApplicationDbContext _db;
    public ProjectReviewRepository(ApplicationDbContext db) => _db = db;

    public async Task<IReadOnlyList<ProjectReview>> GetByProjectIdAsync(
        Guid projectId, CancellationToken ct = default) =>
        await _db.ProjectReviews
            .Include(r => r.ReviewedBy)
            .Where(r => r.ProjectId == projectId)
            .OrderByDescending(r => r.ReviewedAt)
            .ToListAsync(ct);

    public async Task<ProjectReview> AddAsync(ProjectReview review, CancellationToken ct = default)
    {
        _db.ProjectReviews.Add(review);
        await _db.SaveChangesAsync(ct);
        return review;
    }
}
```

- [ ] Create `IReviewService.cs`:

```csharp
using CapstoneRegistration.API.DTOs.Requests;
using CapstoneRegistration.API.DTOs.Responses;

namespace CapstoneRegistration.API.Services.Interfaces;

public interface IReviewService
{
    Task<ReviewResponse> SubmitReviewAsync(
        Guid projectId, Guid reviewerId, ReviewRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<ReviewResponse>> GetReviewsByProjectAsync(
        Guid projectId, CancellationToken ct = default);
}
```

- [ ] Create `ReviewService.cs`:

```csharp
using CapstoneRegistration.API.Data;
using CapstoneRegistration.API.DTOs.Requests;
using CapstoneRegistration.API.DTOs.Responses;
using CapstoneRegistration.API.Exceptions;
using CapstoneRegistration.API.Models;
using CapstoneRegistration.API.Repositories.Interfaces;
using CapstoneRegistration.API.Services.Interfaces;

namespace CapstoneRegistration.API.Services.Implementations;

public class ReviewService : IReviewService
{
    private readonly IProjectReviewRepository _reviewRepo;
    private readonly ICapstoneProjectRepository _projectRepo;

    public ReviewService(
        IProjectReviewRepository reviewRepo,
        ICapstoneProjectRepository projectRepo)
    {
        _reviewRepo  = reviewRepo;
        _projectRepo = projectRepo;
    }

    public async Task<ReviewResponse> SubmitReviewAsync(
        Guid projectId, Guid reviewerId, ReviewRequest request, CancellationToken ct = default)
    {
        if (request.Decision is not ("Accepted" or "Denied"))
            throw new BadRequestException("Decision must be 'Accepted' or 'Denied'.");

        // Verify the project exists
        var project = await _projectRepo.GetByIdWithDetailsAsync(projectId, ct)
            ?? throw new NotFoundException("CapstoneProject", projectId);

        if (project.Status != "Pending")
            throw new BadRequestException($"Project is already '{project.Status}' and cannot be reviewed again.");

        var review = new ProjectReview
        {
            ProjectId    = projectId,
            ReviewedById = reviewerId,
            Decision     = request.Decision,
            Comment      = request.Comment,
            ReviewedAt   = DateTime.UtcNow
        };

        await _reviewRepo.AddAsync(review, ct);

        // Update project status to match the decision
        project.Status    = request.Decision; // "Accepted" or "Denied"
        project.UpdatedAt = DateTime.UtcNow;
        // (No separate Update method needed — EF Core tracks the loaded entity)

        return new ReviewResponse
        {
            Id             = review.Id,
            ProjectId      = review.ProjectId,
            ReviewedById   = review.ReviewedById,
            ReviewedByName = project.CreatedBy?.FullName ?? "Unknown",
            Decision       = review.Decision,
            Comment        = review.Comment,
            ReviewedAt     = review.ReviewedAt
        };
    }

    public async Task<IReadOnlyList<ReviewResponse>> GetReviewsByProjectAsync(
        Guid projectId, CancellationToken ct = default)
    {
        var reviews = await _reviewRepo.GetByProjectIdAsync(projectId, ct);
        return reviews.Select(r => new ReviewResponse
        {
            Id             = r.Id,
            ProjectId      = r.ProjectId,
            ReviewedById   = r.ReviewedById,
            ReviewedByName = r.ReviewedBy.FullName,
            Decision       = r.Decision,
            Comment        = r.Comment,
            ReviewedAt     = r.ReviewedAt
        }).ToList();
    }
}
```

- [ ] Create `ReviewController.cs`:

```csharp
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CapstoneRegistration.API.Common;
using CapstoneRegistration.API.DTOs.Requests;
using CapstoneRegistration.API.Exceptions;
using CapstoneRegistration.API.Services.Interfaces;

namespace CapstoneRegistration.API.Controllers;

[ApiController]
[Route("api/projects/{projectId:guid}/reviews")]
[Authorize]
public class ReviewController : ControllerBase
{
    private readonly IReviewService _reviewService;
    public ReviewController(IReviewService reviewService) => _reviewService = reviewService;

    /// <summary>
    /// Submit a review decision (Accepted/Denied) for a project.
    /// Any authenticated lecturer may review any Pending project.
    /// Each review is stored in history. The project status is updated to match the decision.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Submit(
        Guid projectId,
        [FromBody] ReviewRequest request,
        CancellationToken ct)
    {
        var reviewerId = GetCurrentUserId();
        var result = await _reviewService.SubmitReviewAsync(projectId, reviewerId, request, ct);
        return Ok(ApiResponse<object>.Ok(result, $"Project {result.Decision.ToLower()} successfully."));
    }

    /// <summary>Get the full review history for a project.</summary>
    [HttpGet]
    public async Task<IActionResult> GetHistory(Guid projectId, CancellationToken ct)
    {
        var result = await _reviewService.GetReviewsByProjectAsync(projectId, ct);
        return Ok(ApiResponse<object>.Ok(result));
    }

    private Guid GetCurrentUserId()
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier)
               ?? User.FindFirstValue("sub");
        if (sub is null || !Guid.TryParse(sub, out var userId))
            throw new UnauthorizedException("Cannot determine current user.");
        return userId;
    }
}
```

- [ ] In `ServiceCollectionExtensions.cs` add both:
  ```
  services.AddScoped<IProjectReviewRepository, ProjectReviewRepository>();
  services.AddScoped<IReviewService, ReviewService>();
  ```

- [ ] Commit: `feat: add Review feature (repository, service, controller)`

---

## Task 10: Verify build + final commit

- [ ] Run `dotnet build` — expect 0 errors, 0 warnings
- [ ] Fix any compile errors
- [ ] Commit: `chore: verify full feature layer build passes`
