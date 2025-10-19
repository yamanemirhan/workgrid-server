using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Domain.Entities;

namespace Infrastructure.Data.Configurations;

public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.ToTable("Invoices");
        
        builder.HasKey(i => i.Id);
        
        builder.Property(i => i.InvoiceNumber)
            .IsRequired()
            .HasMaxLength(50);
            
        builder.Property(i => i.Amount)
            .IsRequired()
            .HasColumnType("decimal(10,2)");
            
        builder.Property(i => i.Currency)
            .IsRequired()
            .HasMaxLength(3)
            .HasDefaultValue("USD");
            
        builder.Property(i => i.Status)
            .IsRequired()
            .HasMaxLength(20)
            .HasDefaultValue("pending");
            
        builder.Property(i => i.StripeInvoiceId)
            .HasMaxLength(100);
        
        // Navigation properties
        builder.HasOne(i => i.Workspace)
            .WithMany()
            .HasForeignKey(i => i.WorkspaceId)
            .OnDelete(DeleteBehavior.Cascade);
            
        // Indexes
        builder.HasIndex(i => i.InvoiceNumber)
            .IsUnique();
        builder.HasIndex(i => i.StripeInvoiceId);
        builder.HasIndex(i => i.WorkspaceId);
    }
}