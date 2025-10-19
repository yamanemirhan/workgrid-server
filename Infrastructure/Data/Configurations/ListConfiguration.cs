using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Domain.Entities;

namespace Infrastructure.Data.Configurations;

public class ListConfiguration : IEntityTypeConfiguration<List>
{
    public void Configure(EntityTypeBuilder<List> builder)
    {
        builder.ToTable("Lists");
        
        builder.HasKey(l => l.Id);
        
        builder.Property(l => l.Title)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(l => l.Position)
            .IsRequired();

        builder.Property(l => l.CreatedBy)
            .IsRequired();
        
        // Navigation properties
        builder.HasOne(l => l.Board)
            .WithMany(b => b.Lists)
            .HasForeignKey(l => l.BoardId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(l => l.Creator)
            .WithMany()
            .HasForeignKey(l => l.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.HasMany(l => l.Cards)
            .WithOne(c => c.List)
            .HasForeignKey(c => c.ListId)
            .OnDelete(DeleteBehavior.Cascade);
            
        // Index for ordering
        builder.HasIndex(l => new { l.BoardId, l.Position });
    }
}