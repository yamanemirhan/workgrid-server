using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Domain.Entities;

namespace Infrastructure.Data.Configurations;

public class WorkspaceMemberConfiguration : IEntityTypeConfiguration<WorkspaceMember>
{
    public void Configure(EntityTypeBuilder<WorkspaceMember> builder)
    {
        builder.ToTable("WorkspaceMembers");
        
        builder.HasKey(wm => wm.Id);
        
        builder.Property(wm => wm.Role)
            .IsRequired()
            .HasConversion<string>();
        
        // Composite unique index for UserId + WorkspaceId
        builder.HasIndex(wm => new { wm.UserId, wm.WorkspaceId })
            .IsUnique();
            
        // Navigation properties are configured in User and Workspace configurations
    }
}