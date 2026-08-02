using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Sibers.Core.Entities;

namespace Sibers.Infrastructure.Data;

/// <summary>
/// Application database context supporting Entity Framework Core and ASP.NET Core Identity.
/// </summary>
public class AppDbContext : IdentityDbContext<IdentityUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Project> Projects => Set<Project>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<ProjectEmployee> ProjectEmployees => Set<ProjectEmployee>();
    public DbSet<ProjectTask> Tasks => Set<ProjectTask>();
    public DbSet<ProjectDocument> Documents => Set<ProjectDocument>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configure ProjectEmployee composite key and relationships
        builder.Entity<ProjectEmployee>(entity =>
        {
            entity.HasKey(pe => new { pe.ProjectId, pe.EmployeeId });

            entity.HasOne(pe => pe.Project)
                .WithMany(p => p.ProjectEmployees)
                .HasForeignKey(pe => pe.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(pe => pe.Employee)
                .WithMany(e => e.ProjectEmployees)
                .HasForeignKey(pe => pe.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure Project relationship with ProjectManager (Employee)
        builder.Entity<Project>(entity =>
        {
            entity.HasOne(p => p.ProjectManager)
                .WithMany(e => e.ManagedProjects)
                .HasForeignKey(p => p.ProjectManagerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure ProjectTask relationships with Author and Executor
        builder.Entity<ProjectTask>(entity =>
        {
            entity.HasOne(t => t.Project)
                .WithMany(p => p.Tasks)
                .HasForeignKey(t => t.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(t => t.Author)
                .WithMany(e => e.AuthoredTasks)
                .HasForeignKey(t => t.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(t => t.Executor)
                .WithMany(e => e.ExecutedTasks)
                .HasForeignKey(t => t.ExecutorId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure ProjectDocument relationship
        builder.Entity<ProjectDocument>(entity =>
        {
            entity.HasOne(d => d.Project)
                .WithMany(p => p.Documents)
                .HasForeignKey(d => d.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
