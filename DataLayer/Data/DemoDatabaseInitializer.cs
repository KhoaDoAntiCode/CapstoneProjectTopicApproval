using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using CapstoneRegistration.API.Models;
using System.Data.Common;
using System.Text.RegularExpressions;

namespace CapstoneRegistration.API.Data;

public static class DemoDatabaseInitializer
{
    public static async Task InitializeAsync(IServiceProvider services, CancellationToken ct = default)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await db.Database.OpenConnectionAsync(ct);

        try
        {
            if (!await UsersTableExistsAsync(db, ct))
            {
                await ExecuteSchemaScriptAsync(db, ct);
            }
        }
        finally
        {
            await db.Database.CloseConnectionAsync();
        }

        const string adminEmail = "admin@local.demo";
        var adminExists = await db.Users.AnyAsync(x => x.Email == adminEmail, ct);
        if (adminExists)
        {
            return;
        }

        db.Users.Add(new User
        {
            Username = "admin",
            Email = adminEmail,
            FullName = "System Admin",
            Password = "admin123"
        });

        await db.SaveChangesAsync(ct);
    }

    private static async Task<bool> UsersTableExistsAsync(ApplicationDbContext db, CancellationToken ct)
    {
        await using var command = db.Database.GetDbConnection().CreateCommand();
        command.CommandText = """
            SELECT CASE
                WHEN EXISTS (
                    SELECT 1
                    FROM INFORMATION_SCHEMA.TABLES
                    WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'users'
                ) THEN 1
                ELSE 0
            END
            """;

        var result = await command.ExecuteScalarAsync(ct);
        return Convert.ToInt32(result) == 1;
    }

    private static async Task ExecuteSchemaScriptAsync(ApplicationDbContext db, CancellationToken ct)
    {
        var scriptPath = ResolveSchemaScriptPath();
        var script = await File.ReadAllTextAsync(scriptPath, ct);
        var batches = Regex.Split(script, @"^\s*GO\s*($|\-\-.*$)", RegexOptions.Multiline | RegexOptions.IgnoreCase)
            .Where(batch => !string.IsNullOrWhiteSpace(batch))
            .ToList();

        foreach (var batch in batches)
        {
            await using var command = db.Database.GetDbConnection().CreateCommand();
            command.CommandText = batch;
            await command.ExecuteNonQueryAsync(ct);
        }
    }

    private static string ResolveSchemaScriptPath()
    {
        var candidates = new[]
        {
            Path.Combine(Directory.GetCurrentDirectory(), "Database", "sqlserver-demo-schema.sql"),
            Path.Combine(AppContext.BaseDirectory, "Database", "sqlserver-demo-schema.sql"),
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "CapstoneRegistration.API", "Database", "sqlserver-demo-schema.sql")
        };

        var found = candidates.Select(Path.GetFullPath).FirstOrDefault(File.Exists);
        if (found is null)
        {
            throw new FileNotFoundException("Could not find sqlserver-demo-schema.sql for database initialization.");
        }

        return found;
    }
}
