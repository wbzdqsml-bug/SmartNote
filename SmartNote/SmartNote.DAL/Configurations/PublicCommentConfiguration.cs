using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartNote.Domain.Entities;

namespace SmartNote.DAL.Configurations
{
    public class PublicCommentConfiguration : IEntityTypeConfiguration<PublicComment>
    {
        public void Configure(EntityTypeBuilder<PublicComment> builder)
        {
            builder.ToTable("PublicComments");

            builder.HasQueryFilter(comment => !comment.PublicContent.Note.IsDeleted);

            builder.HasOne(pc => pc.PublicContent)
                .WithMany(content => content.Comments)
                .HasForeignKey(pc => pc.PublicContentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(pc => pc.AuthorUser)
                .WithMany(u => u.PublicComments)
                .HasForeignKey(pc => pc.AuthorUserId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(pc => pc.Parent)
                .WithMany(parent => parent.Replies)
                .HasForeignKey(pc => pc.ParentId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
