using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Domain.Entities;

namespace Infrastructure.Data.Configurations;

public class CardFollowerConfiguration : IEntityTypeConfiguration<CardFollower>
{
    public void Configure(EntityTypeBuilder<CardFollower> builder)
    {
        builder.HasKey(cf => cf.Id);

        builder.Property(cf => cf.FollowedAt)
            .IsRequired();

        // CardFollower-Card relationship
        builder.HasOne(cf => cf.Card)
            .WithMany(c => c.CardFollowers)
            .HasForeignKey(cf => cf.CardId)
            .OnDelete(DeleteBehavior.Cascade);

        // CardFollower-User relationship
        builder.HasOne(cf => cf.User)
            .WithMany()
            .HasForeignKey(cf => cf.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Unique constraint: one user can follow a card only once
        builder.HasIndex(cf => new { cf.CardId, cf.UserId })
            .IsUnique();

        builder.ToTable("CardFollowers");
    }
}