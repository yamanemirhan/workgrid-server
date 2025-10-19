using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Domain.Entities;

namespace Infrastructure.Data.Configurations;

public class WorkspaceInvitationConfiguration : IEntityTypeConfiguration<WorkspaceInvitation>
{
    public void Configure(EntityTypeBuilder<WorkspaceInvitation> builder)
    {
        builder.ToTable("WorkspaceInvitations");
        
        builder.HasKey(wi => wi.Id);
        
        builder.Property(wi => wi.Email)
            .IsRequired()
            .HasMaxLength(256);
            
        builder.Property(wi => wi.Token)
            .IsRequired()
            .HasMaxLength(128);
        
        builder.Property(wi => wi.Role)
            .IsRequired()
            .HasConversion<string>();
            
        builder.Property(wi => wi.Status)
            .IsRequired()
            .HasConversion<string>();
        
        // Configure foreign key to User (InvitedBy)
        builder.HasOne(wi => wi.InvitedBy)
            .WithMany()
            .HasForeignKey(wi => wi.InvitedByUserId)
            .OnDelete(DeleteBehavior.Restrict); // Don't cascade delete if user is deleted
        
        // Configure foreign key to Workspace
        builder.HasOne(wi => wi.Workspace)
            .WithMany()
            .HasForeignKey(wi => wi.WorkspaceId)
            .OnDelete(DeleteBehavior.Cascade); // Delete invitations when workspace is deleted
        
        // Indexes
        builder.HasIndex(wi => wi.Token)
            .IsUnique();
            
        builder.HasIndex(wi => new { wi.WorkspaceId, wi.Email })
            .HasDatabaseName("IX_WorkspaceInvitations_WorkspaceId_Email");
    }
}