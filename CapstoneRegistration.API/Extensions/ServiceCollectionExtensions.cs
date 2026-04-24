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
        var connectionString = BuildConnectionString(configuration);
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped(typeof(IBaseRepository<,>), typeof(BaseRepository<,>));
        services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));

        services.AddScoped<ICapstoneProjectRepository, CapstoneProjectRepository>();
        services.AddScoped<IProjectReviewRepository, ProjectReviewRepository>();

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IDocxParserService, DocxParserService>();
        services.AddScoped<IProjectService, ProjectService>();
        services.AddScoped<IReviewService, ReviewService>();

        return services;
    }

    /// <summary>
    /// Builds the Npgsql connection string.
    ///
    /// Priority:
    ///   1. DATABASE_URL env var — Railway's standard postgres URL
    ///      e.g. postgresql://user:pass@host:5432/dbname
    ///   2. ConnectionStrings:DefaultConnection in appsettings.json (local dev)
    /// </summary>
    private static string BuildConnectionString(IConfiguration configuration)
    {
        var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");

        if (!string.IsNullOrWhiteSpace(databaseUrl))
        {
            // Parse postgresql://user:password@host:port/database
            var uri      = new Uri(databaseUrl);
            var userInfo = uri.UserInfo.Split(':', 2);
            var username = userInfo[0];
            var password = userInfo.Length > 1 ? userInfo[1] : string.Empty;
            var host     = uri.Host;
            var port     = uri.Port > 0 ? uri.Port : 5432;
            var database = uri.AbsolutePath.TrimStart('/');

            var connectionString = $"Host={host};Port={port};Database={database};Username={username};Password={password}";
            Console.WriteLine($"Database connection string: {connectionString}");

            return connectionString;
        }

        // Fall back to appsettings.json (local development)
        return configuration.GetConnectionString("DefaultConnection")
               ?? throw new InvalidOperationException(
                   "No database connection configured. " +
                   "Set DATABASE_URL env var or ConnectionStrings:DefaultConnection in appsettings.json.");
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
