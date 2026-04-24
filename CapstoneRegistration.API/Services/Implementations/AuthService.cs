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
        var exists = await _db.Users.AnyAsync(u => u.Email == request.Email.ToLower(), ct);
        if (exists)
            throw new BadRequestException("An account with this email already exists.");

        var user = new User
        {
            Email           = request.Email.ToLower(),
            FullName        = request.FullName,
            Role            = "Admin",
            Password        = request.Password,
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

        if (user is null || !string.Equals(request.Password, user.Password, StringComparison.Ordinal))
            throw new UnauthorizedException("Invalid email or password.");

        return BuildAuthResponse(user);
    }

    private AuthResponse BuildAuthResponse(User user)
    {
        var jwtSection = _config.GetSection("Jwt");
        var key        = jwtSection["Key"]!;
        var issuer     = jwtSection["Issuer"]!;
        var audience   = jwtSection["Audience"]!;
        var expiryMins = int.Parse(jwtSection["ExpiryMinutes"] ?? "60");
        var expiresAt  = DateTime.UtcNow.AddMinutes(expiryMins);

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
