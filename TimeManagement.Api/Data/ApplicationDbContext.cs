using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TimeManagement.Api.Models;

namespace TimeManagement.Api.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<UserTask> Tasks { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<TaskHistory> TaskHistories { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<UserTask>()
            .HasOne(t => t.Creator)
            .WithMany()
            .HasForeignKey(t => t.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<UserTask>()
            .HasOne(t => t.Assignee)
            .WithMany()
            .HasForeignKey(t => t.AssignedTo)
            .OnDelete(DeleteBehavior.SetNull);

        // Configure many-to-many relationship between UserTask and Tag
        builder.Entity<UserTask>()
            .HasMany(t => t.Tags)
            .WithMany(tag => tag.Tasks);

        builder.Entity<TaskHistory>()
            .HasOne(h => h.Task)
            .WithMany(t => t.History)
            .HasForeignKey(h => h.TaskId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<TaskHistory>()
            .HasOne(h => h.PerformedByUser)
            .WithMany()
            .HasForeignKey(h => h.PerformedBy)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
