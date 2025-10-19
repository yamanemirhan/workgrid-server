using Microsoft.EntityFrameworkCore;
using Domain.Entities;

namespace ActivityService.Data;

public class ActivityDbContext : DbContext
{
    public ActivityDbContext(DbContextOptions<ActivityDbContext> options)
        : base(options)
    {
    }

    public DbSet<Activity> Activities { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Activity configuration
        modelBuilder.Entity<Activity>(entity =>
        {
            entity.ToTable("Activities");
            entity.HasKey(a => a.Id);

            entity.Property(a => a.Type)
                .IsRequired()
                .HasConversion<string>();

            entity.Property(a => a.Description)
                .HasMaxLength(1000);

            entity.Property(a => a.EntityType)
                .HasMaxLength(100);

            entity.Property(a => a.Metadata)
                .HasColumnType("jsonb");

            entity.Property(a => a.CreatedAt)
                .IsRequired();

            entity.Property(a => a.WorkspaceId).IsRequired();
            entity.Property(a => a.BoardId).IsRequired(false);
            entity.Property(a => a.ListId).IsRequired(false);
            entity.Property(a => a.CardId).IsRequired(false);
            entity.Property(a => a.UserId).IsRequired(false);
            entity.Property(a => a.EntityId).IsRequired(false);

            entity.Ignore(a => a.User);
            entity.Ignore(a => a.Workspace);
            entity.Ignore(a => a.Board);
            entity.Ignore(a => a.List);
            entity.Ignore(a => a.Card);

            entity.HasIndex(a => new { a.WorkspaceId, a.CreatedAt });
            entity.HasIndex(a => a.CreatedAt);
            entity.HasIndex(a => a.BoardId);
            entity.HasIndex(a => a.CardId);
            entity.HasIndex(a => a.Type);
            entity.HasIndex(a => a.UserId);
        });

        modelBuilder.Ignore<User>();
        modelBuilder.Ignore<Workspace>();
        modelBuilder.Ignore<WorkspaceMember>();
        modelBuilder.Ignore<WorkspaceInvitation>();
        modelBuilder.Ignore<Board>();
        modelBuilder.Ignore<BoardMember>();
        modelBuilder.Ignore<List>();
        modelBuilder.Ignore<Card>();
        modelBuilder.Ignore<CardMember>();
        modelBuilder.Ignore<CardFollower>();
        modelBuilder.Ignore<CardStatus>();
        modelBuilder.Ignore<Notification>();
        modelBuilder.Ignore<Comment>();
        modelBuilder.Ignore<CommentAttachment>();
        modelBuilder.Ignore<CommentMention>();
        modelBuilder.Ignore<CommentReaction>();
        modelBuilder.Ignore<Subscription>();
        modelBuilder.Ignore<Invoice>();
    }
}