using Microsoft.EntityFrameworkCore;
using CapstoneRegistration.API.Models;

namespace CapstoneRegistration.API.Data;

public static class DemoDatabaseInitializer
{
    public static async Task InitializeAsync(IServiceProvider services, CancellationToken ct = default)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await db.Database.EnsureCreatedAsync(ct);

        const string adminEmail = "admin@local.demo";
        var adminExists = await db.Users.AnyAsync(x => x.Email == adminEmail, ct);
        if (adminExists)
        {
            return;
        }

        db.Users.Add(new User
        {
            Email = adminEmail,
            FullName = "System Admin",
            Role = "Admin",
            Password = "admin123",
            IsEmailVerified = true
        });

        await db.SaveChangesAsync(ct);
    }
}
