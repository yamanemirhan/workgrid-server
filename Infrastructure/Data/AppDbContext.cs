using Microsoft.EntityFrameworkCore;
using Domain.Entities;

namespace Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // DbSets
    public DbSet<User> Users { get; set; }
    public DbSet<Workspace> Workspaces { get; set; }
    public DbSet<WorkspaceMember> WorkspaceMembers { get; set; }
    public DbSet<WorkspaceInvitation> WorkspaceInvitations { get; set; }
    public DbSet<Board> Boards { get; set; }
    public DbSet<BoardMember> BoardMembers { get; set; }
    public DbSet<List> Lists { get; set; }
    public DbSet<Card> Cards { get; set; }
    public DbSet<CardMember> CardMembers { get; set; }
    public DbSet<CardFollower> CardFollowers { get; set; }
    public DbSet<CardStatus> CardStatuses { get; set; }
    public DbSet<Activity> Activities { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<Subscription> Subscriptions { get; set; }
    public DbSet<Invoice> Invoices { get; set; }

    // Comment-related DbSets
    public DbSet<Comment> Comments { get; set; }
    public DbSet<CommentAttachment> CommentAttachments { get; set; }
    public DbSet<CommentMention> CommentMentions { get; set; }
    public DbSet<CommentReaction> CommentReactions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply configurations
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker
            .Entries()
            .Where(e => e.Entity is BaseEntity && (
                    e.State == EntityState.Added
                    || e.State == EntityState.Modified));

        foreach (var entityEntry in entries)
        {
            var entity = (BaseEntity)entityEntry.Entity;
            entity.UpdatedAt = DateTime.UtcNow;

            if (entityEntry.State == EntityState.Added)
            {
                entity.CreatedAt = DateTime.UtcNow;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}