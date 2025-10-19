using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Domain.Entities;

namespace Infrastructure.Data.Configurations;

public class WorkspaceConfiguration : IEntityTypeConfiguration<Workspace>
{
    public void Configure(EntityTypeBuilder<Workspace> builder)
    {
        builder.ToTable("Workspaces");
        
        builder.HasKey(w => w.Id);
        
        builder.Property(w => w.Name)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(w => w.Description)
            .HasMaxLength(500);
            
        builder.Property(w => w.Logo)
            .HasMaxLength(500);
        
        // Navigation properties
        builder.HasOne(w => w.Owner)
            .WithMany(u => u.OwnedWorkspaces)
            .HasForeignKey(w => w.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.HasMany(w => w.Members)
            .WithOne(wm => wm.Workspace)
            .HasForeignKey(wm => wm.WorkspaceId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasMany(w => w.Boards)
            .WithOne(b => b.Workspace)
            .HasForeignKey(b => b.WorkspaceId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasMany(w => w.Activities)
            .WithOne(a => a.Workspace)
            .HasForeignKey(a => a.WorkspaceId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasOne(w => w.Subscription)
            .WithOne(s => s.Workspace)
            .HasForeignKey<Subscription>(s => s.WorkspaceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}