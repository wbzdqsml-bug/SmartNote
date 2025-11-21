using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartNote.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartNote.DAL.Configurations
{
    public class TagConfiguration : IEntityTypeConfiguration<Tag>
    {
        public void Configure(EntityTypeBuilder<Tag> builder)
        {
            builder.ToTable("Tags");

            builder.Property(t => t.Name)
                   .IsRequired()
                   .HasMaxLength(30);

            builder.Property(t => t.Color)
                   .HasMaxLength(10);

            builder.HasIndex(t => new { t.UserId, t.Name })
                   .IsUnique(); // 同一用户下标签名唯一

            builder.HasOne(t => t.User)
                   .WithMany(u => u.Tags)
                   .HasForeignKey(t => t.UserId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
