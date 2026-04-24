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
    public DbSet<StudentGroup> StudentGroups => Set<StudentGroup>();
    public DbSet<StudentGroupMember> StudentGroupMembers => Set<StudentGroupMember>();
    public DbSet<ProjectReview> ProjectReviews => Set<ProjectReview>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(e =>
        {
            e.HasIndex(u => u.Email)
                .HasDatabaseName("IX_admin_users_email");

            e.HasIndex(u => u.Username)
                .HasDatabaseName("UQ_admin_users_username")
                .IsUnique();
        });

        modelBuilder.Entity<CapstoneProject>(e =>
        {
            e.HasIndex(p => p.ProjectCode)
                .HasDatabaseName("UQ_capstone_submissions_submission_code")
                .IsUnique();

            e.HasOne(p => p.CreatedBy)
                .WithMany(u => u.CreatedProjects)
                .HasForeignKey(p => p.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(p => p.Group)
                .WithMany()
                .HasForeignKey(p => p.GroupId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(p => p.Supervisor)
                .WithMany()
                .HasForeignKey(p => p.SupervisorId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<StudentGroup>(e =>
        {
            e.HasOne(g => g.CreatedByAdmin)
                .WithMany()
                .HasForeignKey(g => g.CreatedByAdminId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<StudentGroupMember>(e =>
        {
            e.HasOne(m => m.Group)
                .WithMany(g => g.Members)
                .HasForeignKey(m => m.GroupId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(m => m.Student)
                .WithMany()
                .HasForeignKey(m => m.StudentId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
