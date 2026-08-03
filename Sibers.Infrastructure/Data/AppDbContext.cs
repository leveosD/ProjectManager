using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Sibers.Core.Entities;
using Sibers.Core.Interfaces;
using Sibers.Infrastructure.Services;

namespace Sibers.Infrastructure.Data;

/// <summary>
/// Application database context supporting Entity Framework Core and ASP.NET Core Identity.
/// </summary>
public class AppDbContext : IdentityDbContext<IdentityUser>
{
    private readonly ICurrentUserService _currentUserService;
    
    public AppDbContext(DbContextOptions<AppDbContext> options, ICurrentUserService currentUserService) : base(options)
    {
        _currentUserService = currentUserService;
    }

    public DbSet<Project> Projects => Set<Project>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<ProjectEmployee> ProjectEmployees => Set<ProjectEmployee>();
    public DbSet<ProjectTask> Tasks => Set<ProjectTask>();
    public DbSet<ProjectDocument> Documents => Set<ProjectDocument>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configure Employee properties
        builder.Entity<Employee>(entity =>
        {
            entity.Property(e => e.FirstName)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.LastName)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.MiddleName)
                .HasMaxLength(100);

            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(256);

            entity.Property(e => e.UserId)
                .HasMaxLength(450);

            entity.HasIndex(e => e.Email)
                .IsUnique();
        });

        // Configure Project properties
        builder.Entity<Project>(entity =>
        {
            entity.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(p => p.CustomerCompany)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(p => p.ExecutingCompany)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(p => p.StartDate)
                .IsRequired();

            entity.Property(p => p.EndDate)
                .IsRequired();

            entity.Property(p => p.Priority)
                .IsRequired();

            entity.Property(p => p.ProjectManagerId)
                .IsRequired();

            entity.HasOne(p => p.ProjectManager)
                .WithMany(e => e.ManagedProjects)
                .HasForeignKey(p => p.ProjectManagerId)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasQueryFilter(p =>
                _currentUserService.IsAuthenticated &&
                (
                    _currentUserService.Role == Core.Enums.UserRoles.Director ||

                    (_currentUserService.Role == Core.Enums.UserRoles.ProjectManager ||
                     _currentUserService.Role == Core.Enums.UserRoles.Employee) &&
                    (p.ProjectManagerId == _currentUserService.EmployeeId ||
                     p.ProjectEmployees.Any(pe => pe.EmployeeId == _currentUserService.EmployeeId))
                )
            );
        });

        // Configure ProjectTask properties
        builder.Entity<ProjectTask>(entity =>
        {
            entity.Property(t => t.Title)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(t => t.Comment)
                .HasMaxLength(2000);

            entity.Property(t => t.Priority)
                .IsRequired();

            entity.Property(t => t.Status)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50);

            entity.Property(t => t.ProjectId)
                .IsRequired();

            entity.Property(t => t.AuthorId)
                .IsRequired();

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
            
            entity.HasQueryFilter(t =>
                _currentUserService.IsAuthenticated &&
                (
                    _currentUserService.Role == Core.Enums.UserRoles.Director ||

                    (_currentUserService.Role == Core.Enums.UserRoles.ProjectManager &&
                     (t.Project.ProjectManagerId == _currentUserService.EmployeeId ||
                      t.AuthorId == _currentUserService.EmployeeId ||
                      t.ExecutorId == _currentUserService.EmployeeId)) ||

                    (_currentUserService.Role == Core.Enums.UserRoles.Employee &&
                     (t.AuthorId == _currentUserService.EmployeeId ||
                      t.ExecutorId == _currentUserService.EmployeeId))
                )
            );
        });

        // Configure ProjectDocument properties
        builder.Entity<ProjectDocument>(entity =>
        {
            entity.Property(d => d.FileName)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(d => d.StoredFileName)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(d => d.ContentType)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(d => d.FileSize)
                .IsRequired();

            entity.Property(d => d.UploadedAt)
                .IsRequired();

            entity.Property(d => d.ProjectId)
                .IsRequired();

            entity.HasOne(d => d.Project)
                .WithMany(p => p.Documents)
                .HasForeignKey(d => d.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure ProjectEmployee composite key and relationships
        builder.Entity<ProjectEmployee>(entity =>
        {
            entity.HasKey(pe => new { pe.ProjectId, pe.EmployeeId });

            entity.Property(pe => pe.ProjectId)
                .IsRequired();

            entity.Property(pe => pe.EmployeeId)
                .IsRequired();

            entity.HasOne(pe => pe.Project)
                .WithMany(p => p.ProjectEmployees)
                .HasForeignKey(pe => pe.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(pe => pe.Employee)
                .WithMany(e => e.ProjectEmployees)
                .HasForeignKey(pe => pe.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
