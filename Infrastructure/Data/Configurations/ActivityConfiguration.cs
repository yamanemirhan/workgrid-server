//using Microsoft.EntityFrameworkCore;
//using Microsoft.EntityFrameworkCore.Metadata.Builders;
//using Domain.Entities;

//namespace Infrastructure.Data.Configurations
//{
//    public class ActivityConfiguration : IEntityTypeConfiguration<Activity>
//    {
//        public void Configure(EntityTypeBuilder<Activity> builder)
//        {
//            builder.ToTable("Activities");

//            builder.HasKey(a => a.Id);

//            builder.Property(a => a.Type)
//                .IsRequired()
//                .HasConversion<string>();

//            builder.Property(a => a.Description)
//                .IsRequired()
//                .HasMaxLength(500);

//            builder.Property(a => a.EntityType)
//                .IsRequired()
//                .HasConversion<string>();

//            builder.Property(a => a.Metadata)
//                .HasColumnType("json");

//            // Navigation properties
//            builder.HasOne(a => a.User)
//                .WithMany(u => u.Activities)
//                .HasForeignKey(a => a.UserId)
//                .OnDelete(DeleteBehavior.Cascade);

//            builder.HasOne(a => a.Workspace)
//                .WithMany(w => w.Activities)
//                .HasForeignKey(a => a.WorkspaceId)
//                .OnDelete(DeleteBehavior.Cascade);

//            // Indexes for queries
//            builder.HasIndex(a => a.WorkspaceId);
//            builder.HasIndex(a => new { a.EntityType, a.EntityId });
//            builder.HasIndex(a => a.CreatedAt);
//        }
//    }
//}
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Domain.Entities;

namespace Infrastructure.Data.Configurations
{
    public class ActivityConfiguration : IEntityTypeConfiguration<Activity>
    {
        public void Configure(EntityTypeBuilder<Activity> builder)
        {
            builder.ToTable("Activities");
            builder.HasKey(a => a.Id);

            builder.Property(a => a.Type)
                .IsRequired()
                .HasConversion<string>();

            builder.Property(a => a.Description)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(a => a.EntityType)
                .IsRequired()
                .HasConversion<string>();

            builder.Property(a => a.Metadata)
                .HasColumnType("json");

            // Navigation properties
            builder.HasOne(a => a.User)
                .WithMany(u => u.Activities)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(a => a.Workspace)
                .WithMany(w => w.Activities)
                .HasForeignKey(a => a.WorkspaceId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(a => a.Board)
                .WithMany()
                .HasForeignKey(a => a.BoardId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(a => a.List)
                .WithMany()
                .HasForeignKey(a => a.ListId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(a => a.Card)
                .WithMany()
                .HasForeignKey(a => a.CardId)
                .OnDelete(DeleteBehavior.Cascade);

            // EntityId için foreign key YOK - polymorphic field

            // Indexes for queries
            builder.HasIndex(a => a.WorkspaceId);
            builder.HasIndex(a => new { a.EntityType, a.EntityId });
            builder.HasIndex(a => a.CreatedAt);
        }
    }
}