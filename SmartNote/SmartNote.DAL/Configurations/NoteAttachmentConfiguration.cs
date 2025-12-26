using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartNote.Domain.Entities;

namespace SmartNote.DAL.Configurations
{
    public class NoteAttachmentConfiguration : IEntityTypeConfiguration<NoteAttachment>
    {
        public void Configure(EntityTypeBuilder<NoteAttachment> builder)
        {
            builder.HasOne(a => a.Note)
                .WithMany()
                .HasForeignKey(a => a.NoteId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasQueryFilter(a => !a.Note.IsDeleted);

            builder.HasOne(a => a.UploaderUser)
                .WithMany()
                .HasForeignKey(a => a.UploaderUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(a => a.NoteId);
        }
    }
}
