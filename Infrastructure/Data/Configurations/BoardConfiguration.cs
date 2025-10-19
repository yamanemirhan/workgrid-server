using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Domain.Entities;

namespace Infrastructure.Data.Configurations;

public class BoardConfiguration : IEntityTypeConfiguration<Board>
{
    public void Configure(EntityTypeBuilder<Board> builder)
    {
        builder.ToTable("Boards");
        
        builder.HasKey(b => b.Id);
        
        builder.Property(b => b.Title)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(b => b.Description)
            .HasMaxLength(1000);
        
        // Navigation properties
        builder.HasOne(b => b.Workspace)
            .WithMany(w => w.Boards)
            .HasForeignKey(b => b.WorkspaceId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasOne(b => b.Creator)
            .WithMany(u => u.CreatedBoards)
            .HasForeignKey(b => b.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.HasMany(b => b.Lists)
            .WithOne(l => l.Board)
            .HasForeignKey(l => l.BoardId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}