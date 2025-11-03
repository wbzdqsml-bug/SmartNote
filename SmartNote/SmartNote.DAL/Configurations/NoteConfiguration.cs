using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartNote.Domain.Entities;

namespace SmartNote.DAL.Configurations
{
    public class NoteConfiguration : IEntityTypeConfiguration<Note>
    {
        public void Configure(EntityTypeBuilder<Note> builder)
        {
            builder.ToTable("Notes");

            builder.HasKey(n => n.Id);

            builder.Property(n => n.Title)
                   .IsRequired()
                   .HasMaxLength(200);

            builder.Property(n => n.Type)
                   .HasConversion<int>();

            builder.Property(n => n.ContentMd)
                   .HasColumnType("nvarchar(max)");

            builder.Property(n => n.ContentHtml)
                   .HasColumnType("nvarchar(max)");

            builder.Property(n => n.CanvasDataJson)
                   .HasColumnType("nvarchar(max)");

            builder.Property(n => n.CreateTime)
                   .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(n => n.LastUpdateTime)
                   .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(n => n.IsDeleted)
                   .HasDefaultValue(false);

            builder.HasIndex(n => new { n.WorkspaceId, n.LastUpdateTime });

            // 外键：Note → Workspace
            builder.HasOne<Workspace>()
                   .WithMany()
                   .HasForeignKey(n => n.WorkspaceId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
