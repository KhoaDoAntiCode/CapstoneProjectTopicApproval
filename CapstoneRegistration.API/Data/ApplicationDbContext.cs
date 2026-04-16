using Microsoft.EntityFrameworkCore;
using CapstoneRegistration.API.Models;

namespace CapstoneRegistration.API.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Semester> Semesters => Set<Semester>();
    public DbSet<CapstoneProject> CapstoneProjects => Set<CapstoneProject>();
    public DbSet<ProjectSupervisor> ProjectSupervisors => Set<ProjectSupervisor>();
    public DbSet<ProjectStudent> ProjectStudents => Set<ProjectStudent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User
        modelBuilder.Entity<User>(e =>
        {
            e.HasIndex(u => u.Email)
                .HasDatabaseName("idx_users_email")
                .IsUnique();
        });

        // Semester — string PK, no auto-generate
        modelBuilder.Entity<Semester>(e =>
        {
            e.Property(s => s.Id).ValueGeneratedNever();
        });

        // CapstoneProject
        modelBuilder.Entity<CapstoneProject>(e =>
        {
            e.HasIndex(p => p.ProjectCode)
                .HasDatabaseName("idx_capstone_projects_project_code")
                .IsUnique();

            e.HasIndex(p => p.SemesterId)
                .HasDatabaseName("idx_capstone_projects_semester_id");

            e.HasOne(p => p.Semester)
                .WithMany(s => s.CapstoneProjects)
                .HasForeignKey(p => p.SemesterId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(p => p.CreatedBy)
                .WithMany(u => u.CreatedProjects)
                .HasForeignKey(p => p.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ProjectSupervisor — cascade delete when project is deleted
        modelBuilder.Entity<ProjectSupervisor>(e =>
        {
            e.HasOne(ps => ps.Project)
                .WithMany(p => p.Supervisors)
                .HasForeignKey(ps => ps.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ProjectStudent — cascade delete when project is deleted
        modelBuilder.Entity<ProjectStudent>(e =>
        {
            e.HasOne(ps => ps.Project)
                .WithMany(p => p.Students)
                .HasForeignKey(ps => ps.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
