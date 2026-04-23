using Microsoft.EntityFrameworkCore;
using CapstoneRegistration.API.Data;
using CapstoneRegistration.API.DTOs.Requests;
using CapstoneRegistration.API.DTOs.Responses;
using CapstoneRegistration.API.Exceptions;
using CapstoneRegistration.API.Models;
using CapstoneRegistration.API.Repositories.Interfaces;
using CapstoneRegistration.API.Services.Interfaces;
using CapstoneRegistration.API.Common;

namespace CapstoneRegistration.API.Services.Implementations;

public class InstructorService : IInstructorService
{
    private readonly IInstructorRepository _instructorRepository;
    private readonly ApplicationDbContext _context;

    public InstructorService(IInstructorRepository instructorRepository, ApplicationDbContext context)
    {
        _instructorRepository = instructorRepository;
        _context = context;
    }

    public async Task<InstructorResponse> CreateAsync(CreateInstructorRequest request, CancellationToken ct = default)
    {
        // Check if email already exists
        if (await _instructorRepository.EmailExistsAsync(request.Email, null, ct))
            throw new BadRequestException($"Instructor with email '{request.Email}' already exists.");

        var instructor = new Instructor
        {
            Id = Guid.NewGuid(),
            FullName = request.FullName,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            Pronouns = request.Pronouns,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var createdInstructor = await _instructorRepository.AddAsync(instructor, ct);
        await _context.SaveChangesAsync(ct);

        return MapToResponse(createdInstructor);
    }

    public async Task<InstructorResponse> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var instructor = await _instructorRepository.GetByIdAsync(id, ct);
        if (instructor == null)
            throw new NotFoundException($"Instructor with ID '{id}' not found.");

        return MapToResponse(instructor);
    }

    public async Task<PagedResult<InstructorResponse>> GetPagedAsync(int page, int pageSize, string? search = null, CancellationToken ct = default)
    {
        if (page < 1) page = 1;
        if (pageSize is < 1 or > 100) pageSize = 20;

        var query = _context.Instructors.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(i => 
                i.FullName.Contains(search) || 
                i.Email.Contains(search));
        }

        var totalCount = await query.CountAsync(ct);
        var instructors = await query
            .OrderBy(i => i.FullName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var responses = instructors.Select(MapToResponse).ToList();

        return new PagedResult<InstructorResponse>
        {
            Items = responses,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<InstructorResponse> UpdateAsync(Guid id, UpdateInstructorRequest request, CancellationToken ct = default)
    {
        var instructor = await _instructorRepository.GetByIdAsync(id, ct);
        if (instructor == null)
            throw new NotFoundException($"Instructor with ID '{id}' not found.");

        // Check if email already exists for another instructor
        if (await _instructorRepository.EmailExistsAsync(request.Email, id, ct))
            throw new BadRequestException($"Instructor with email '{request.Email}' already exists.");

        instructor.FullName = request.FullName;
        instructor.Email = request.Email;
        instructor.PhoneNumber = request.PhoneNumber;
        instructor.Pronouns = request.Pronouns;
        instructor.UpdatedAt = DateTime.UtcNow;

        await _instructorRepository.UpdateAsync(instructor, ct);
        await _context.SaveChangesAsync(ct);

        return MapToResponse(instructor);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var instructor = await _instructorRepository.GetByIdAsync(id, ct);
        if (instructor == null)
            throw new NotFoundException($"Instructor with ID '{id}' not found.");

        await _instructorRepository.DeleteAsync(instructor, ct);
    }

    private static InstructorResponse MapToResponse(Instructor instructor)
    {
        return new InstructorResponse
        {
            Id = instructor.Id,
            FullName = instructor.FullName,
            Email = instructor.Email,
            PhoneNumber = instructor.PhoneNumber,
            Pronouns = instructor.Pronouns,
            CreatedAt = instructor.CreatedAt,
            UpdatedAt = instructor.UpdatedAt
        };
    }
}
