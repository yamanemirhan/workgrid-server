using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Domain.Entities;

namespace Infrastructure.Data.Configurations;

public class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
{
    public void Configure(EntityTypeBuilder<Subscription> builder)
    {
        builder.ToTable("Subscriptions");
        
        builder.HasKey(s => s.Id);
        
        builder.Property(s => s.Plan)
            .IsRequired()
            .HasConversion<string>();
            
        builder.Property(s => s.Status)
            .IsRequired()
            .HasConversion<string>();
            
        builder.Property(s => s.StripeCustomerId)
            .HasMaxLength(100);
            
        builder.Property(s => s.StripeSubscriptionId)
            .HasMaxLength(100);
        
        // Navigation properties
        builder.HasOne(s => s.Workspace)
            .WithOne(w => w.Subscription)
            .HasForeignKey<Subscription>(s => s.WorkspaceId)
            .OnDelete(DeleteBehavior.Cascade);
            
        // Index for Stripe IDs
        builder.HasIndex(s => s.StripeCustomerId);
        builder.HasIndex(s => s.StripeSubscriptionId);
    }
}