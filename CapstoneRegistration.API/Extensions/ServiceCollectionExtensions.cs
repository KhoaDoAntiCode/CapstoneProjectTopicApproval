using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using CapstoneRegistration.API.Data;
using CapstoneRegistration.API.Repositories.Interfaces;
using CapstoneRegistration.API.Repositories.Implementations;using CapstoneRegistration.API.Services.Interfaces;
using CapstoneRegistration.API.Services.Implementations;
using Microsoft.EntityFrameworkCore;

namespace CapstoneRegistration.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped(typeof(IBaseRepository<,>), typeof(BaseRepository<,>));
        services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));

        services.AddScoped<ICapstoneProjectRepository, CapstoneProjectRepository>();
        services.AddScoped<IProjectReviewRepository, ProjectReviewRepository>();

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IDocxParserService, DocxParserService>();
        services.AddScoped<IProjectDocumentService, ProjectDocumentService>();
        services.AddScoped<IProjectService, ProjectService>();
        services.AddScoped<IReviewService, ReviewService>();

        return services;
    }

    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var jwtSection = configuration.GetSection("Jwt");
        var key = jwtSection["Key"] ?? throw new InvalidOperationException("Jwt:Key is not configured.");
        var issuer = jwtSection["Issuer"] ?? throw new InvalidOperationException("Jwt:Issuer is not configured.");
        var audience = jwtSection["Audience"] ?? throw new InvalidOperationException("Jwt:Audience is not configured.");

        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = issuer,
                    ValidateAudience = true,
                    ValidAudience = audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });

        services.AddAuthorization();

        return services;
    }

    public static IServiceCollection AddCorsPolicy(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var allowedOrigins = configuration
            .GetSection("Cors:AllowedOrigins")
            .Get<string[]>() ?? [];

        services.AddCors(options =>
        {
            options.AddPolicy("DefaultCors", policy =>
            {
                if (allowedOrigins.Length > 0)
                    policy.WithOrigins(allowedOrigins);
                else
                    policy.AllowAnyOrigin();

                policy
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            });
        });

        return services;
    }
}
