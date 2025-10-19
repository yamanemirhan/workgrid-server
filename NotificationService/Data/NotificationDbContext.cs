using Microsoft.EntityFrameworkCore;
using Domain.Entities;

namespace NotificationService.Data;

public class NotificationDbContext : DbContext
{
    public NotificationDbContext(DbContextOptions<NotificationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Notification> Notifications { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Notification configuration
        modelBuilder.Entity<Notification>(entity =>
        {
            entity.ToTable("Notifications");
            entity.HasKey(n => n.Id);

            entity.Property(n => n.Type)
                .IsRequired()
                .HasConversion<string>();

            entity.Property(n => n.Title)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(n => n.Message)
                .IsRequired()
                .HasMaxLength(1000);

            entity.Property(n => n.Data)
                .HasColumnType("jsonb");

            entity.Property(n => n.IsRead)
                .HasDefaultValue(false);

            entity.Property(n => n.CreatedAt)
                .IsRequired();

            entity.Property(n => n.UserId).IsRequired();
            entity.Property(n => n.WorkspaceId).IsRequired();
            entity.Property(n => n.BoardId).IsRequired(false);
            entity.Property(n => n.ListId).IsRequired(false);
            entity.Property(n => n.CardId).IsRequired(false);
            entity.Property(n => n.RelatedUserId).IsRequired(false);

            entity.Ignore(n => n.User);
            entity.Ignore(n => n.RelatedUser);
            entity.Ignore(n => n.Workspace);
            entity.Ignore(n => n.Board);
            entity.Ignore(n => n.List);
            entity.Ignore(n => n.Card);

            entity.HasIndex(n => new { n.UserId, n.IsRead });
            entity.HasIndex(n => n.CreatedAt);
            entity.HasIndex(n => n.WorkspaceId);
            entity.HasIndex(n => n.Type);
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
        modelBuilder.Ignore<Activity>();
        modelBuilder.Ignore<Comment>();
        modelBuilder.Ignore<CommentAttachment>();
        modelBuilder.Ignore<CommentMention>();
        modelBuilder.Ignore<CommentReaction>();
        modelBuilder.Ignore<Subscription>();
        modelBuilder.Ignore<Invoice>();
    }
}