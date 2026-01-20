using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartNote.Domain.Entities;

namespace SmartNote.DAL.Configurations
{
    public class PublicContentStatsConfiguration : IEntityTypeConfiguration<PublicContentStats>
    {
        public void Configure(EntityTypeBuilder<PublicContentStats> builder)
        {
            builder.ToTable("PublicContentStats");
            builder.HasKey(stats => stats.PublicContentId);
        }
    }
}
