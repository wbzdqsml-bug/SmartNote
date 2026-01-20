using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartNote.Domain.Entities;

namespace SmartNote.DAL.Configurations
{
    public class PublicContentReactionConfiguration : IEntityTypeConfiguration<PublicContentReaction>
    {
        public void Configure(EntityTypeBuilder<PublicContentReaction> builder)
        {
            builder.ToTable("PublicContentReactions");
            builder.HasKey(reaction => new { reaction.PublicContentId, reaction.UserId });
            builder.HasQueryFilter(reaction => !reaction.PublicContent.Note.IsDeleted);

            builder.HasOne(reaction => reaction.PublicContent)
                .WithMany(content => content.Reactions)
                .HasForeignKey(reaction => reaction.PublicContentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(reaction => reaction.User)
                .WithMany(user => user.PublicContentReactions)
                .HasForeignKey(reaction => reaction.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
