using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Domain.Entities;

namespace Infrastructure.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        
        builder.HasKey(u => u.Id);
        
        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(255);
            
        builder.Property(u => u.Name)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(u => u.Avatar)
            .HasMaxLength(500);
            
        builder.Property(u => u.PasswordHash)
            .IsRequired()
            .HasMaxLength(255);
        
        builder.HasIndex(u => u.Email)
            .IsUnique();
            
        // Navigation properties
        builder.HasMany(u => u.WorkspaceMembers)
            .WithOne(wm => wm.User)
            .HasForeignKey(wm => wm.UserId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasMany(u => u.OwnedWorkspaces)
            .WithOne(w => w.Owner)
            .HasForeignKey(w => w.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.HasMany(u => u.CreatedBoards)
            .WithOne(b => b.Creator)
            .HasForeignKey(b => b.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.HasMany(u => u.CreatedCards)
            .WithOne(c => c.Creator)
            .HasForeignKey(c => c.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.HasMany(u => u.Activities)
            .WithOne(a => a.User)
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}