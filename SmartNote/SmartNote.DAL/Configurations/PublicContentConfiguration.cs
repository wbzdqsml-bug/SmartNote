using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartNote.Domain.Entities;

namespace SmartNote.DAL.Configurations
{
    public class PublicContentConfiguration : IEntityTypeConfiguration<PublicContent>
    {
        public void Configure(EntityTypeBuilder<PublicContent> builder)
        {
            builder.ToTable("PublicContents");

            builder.HasQueryFilter(pc => !pc.Note.IsDeleted);

            builder.HasOne(pc => pc.Note)
                .WithMany()
                .HasForeignKey(pc => pc.NoteId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(pc => pc.AuthorUser)
                .WithMany(u => u.PublicContents)
                .HasForeignKey(pc => pc.AuthorUserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(pc => pc.Stats)
                .WithOne(stats => stats.PublicContent)
                .HasForeignKey<PublicContentStats>(stats => stats.PublicContentId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
