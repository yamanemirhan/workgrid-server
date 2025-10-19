using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Domain.Entities;

namespace Infrastructure.Data.Configurations;

public class CardMemberConfiguration : IEntityTypeConfiguration<CardMember>
{
    public void Configure(EntityTypeBuilder<CardMember> builder)
    {
        builder.HasKey(cm => cm.Id);

        builder.Property(cm => cm.AssignedAt)
            .IsRequired();

        // CardMember-Card relationship
        builder.HasOne(cm => cm.Card)
            .WithMany(c => c.CardMembers)
            .HasForeignKey(cm => cm.CardId)
            .OnDelete(DeleteBehavior.Cascade);

        // CardMember-User relationship (assigned user)
        builder.HasOne(cm => cm.User)
            .WithMany()
            .HasForeignKey(cm => cm.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // CardMember-User relationship (assigned by user)
        builder.HasOne(cm => cm.AssignedByUser)
            .WithMany()
            .HasForeignKey(cm => cm.AssignedBy)
            .OnDelete(DeleteBehavior.Restrict);

        // Unique constraint: one user can be assigned to a card only once
        builder.HasIndex(cm => new { cm.CardId, cm.UserId })
            .IsUnique();

        builder.ToTable("CardMembers");
    }
}