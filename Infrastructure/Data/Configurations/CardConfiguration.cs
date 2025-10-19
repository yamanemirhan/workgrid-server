using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Domain.Entities;

namespace Infrastructure.Data.Configurations;

public class CardConfiguration : IEntityTypeConfiguration<Card>
{
    public void Configure(EntityTypeBuilder<Card> builder)
    {
        builder.ToTable("Cards");
        
        builder.HasKey(c => c.Id);
        
        builder.Property(c => c.Title)
            .IsRequired()
            .HasMaxLength(200);
            
        builder.Property(c => c.Description)
            .HasMaxLength(2000);
            
        builder.Property(c => c.Position)
            .IsRequired();

        builder.Property(c => c.EstimatedTime)
            .HasMaxLength(50);

        builder.Property(c => c.Tags)
            .HasMaxLength(500);
        
        // Navigation properties
        builder.HasOne(c => c.List)
            .WithMany(l => l.Cards)
            .HasForeignKey(c => c.ListId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasOne(c => c.Creator)
            .WithMany(u => u.CreatedCards)
            .HasForeignKey(c => c.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.HasMany(c => c.Activities)
            .WithOne()
            .HasForeignKey(a => a.EntityId)
            .HasPrincipalKey(c => c.Id)
            .OnDelete(DeleteBehavior.Cascade);
            
        // Index for ordering
        builder.HasIndex(c => new { c.ListId, c.Position });
    }
}