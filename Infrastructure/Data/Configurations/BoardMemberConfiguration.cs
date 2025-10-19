using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Domain.Entities;

namespace Infrastructure.Data.Configurations;

public class BoardMemberConfiguration : IEntityTypeConfiguration<BoardMember>
{
    public void Configure(EntityTypeBuilder<BoardMember> builder)
    {
        builder.HasKey(bm => bm.Id);

        builder.Property(bm => bm.AssignedAt)
            .IsRequired();

        // BoardMember-Board relationship
        builder.HasOne(bm => bm.Board)
            .WithMany(b => b.BoardMembers)
            .HasForeignKey(bm => bm.BoardId)
            .OnDelete(DeleteBehavior.Cascade);

        // BoardMember-User relationship (assigned user)
        builder.HasOne(bm => bm.User)
            .WithMany()
            .HasForeignKey(bm => bm.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // BoardMember-User relationship (assigned by user)
        builder.HasOne(bm => bm.AssignedByUser)
            .WithMany()
            .HasForeignKey(bm => bm.AssignedBy)
            .OnDelete(DeleteBehavior.Restrict);

        // Unique constraint: one user can be assigned to a board only once
        builder.HasIndex(bm => new { bm.BoardId, bm.UserId })
            .IsUnique();

        builder.ToTable("BoardMembers");
    }
}