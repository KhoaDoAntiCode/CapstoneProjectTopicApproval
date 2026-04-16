using Microsoft.EntityFrameworkCore;
using CapstoneRegistration.API.Models;

namespace CapstoneRegistration.API.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<CapstoneProject> CapstoneProjects => Set<CapstoneProject>();
    public DbSet<ProjectSupervisor> ProjectSupervisors => Set<ProjectSupervisor>();
    public DbSet<ProjectStudent> ProjectStudents => Set<ProjectStudent>();
    public DbSet<ProjectReview> ProjectReviews => Set<ProjectReview>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(e =>
        {
            e.HasIndex(u => u.Email)
                .HasDatabaseName("idx_users_email")
                .IsUnique();
        });

        modelBuilder.Entity<CapstoneProject>(e =>
        {
            e.HasIndex(p => p.ProjectCode)
                .HasDatabaseName("idx_capstone_projects_project_code")
                .IsUnique();

            e.HasIndex(p => p.SemesterId)
                .HasDatabaseName("idx_capstone_projects_semester_id");

            e.HasOne(p => p.CreatedBy)
                .WithMany(u => u.CreatedProjects)
                .HasForeignKey(p => p.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ProjectSupervisor>(e =>
        {
            e.HasOne(ps => ps.Project)
                .WithMany(p => p.Supervisors)
                .HasForeignKey(ps => ps.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ProjectStudent>(e =>
        {
            e.HasOne(ps => ps.Project)
                .WithMany(p => p.Students)
                .HasForeignKey(ps => ps.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ProjectReview>(e =>
        {
            e.HasIndex(r => r.ProjectId)
                .HasDatabaseName("idx_project_reviews_project_id");

            e.HasIndex(r => r.ReviewedById)
                .HasDatabaseName("idx_project_reviews_reviewed_by_id");

            e.HasOne(r => r.Project)
                .WithMany(p => p.ProjectReviews)
                .HasForeignKey(r => r.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(r => r.ReviewedBy)
                .WithMany(u => u.Reviews)
                .HasForeignKey(r => r.ReviewedById)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
