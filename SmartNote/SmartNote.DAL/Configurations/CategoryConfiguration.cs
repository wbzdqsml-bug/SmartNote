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
    public class CategoryConfiguration : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> builder)
        {
            builder.ToTable("Categories");

            builder.Property(c => c.Name)
                   .IsRequired()
                   .HasMaxLength(50);

            builder.Property(c => c.Color)
                   .HasMaxLength(10);

            builder.HasIndex(c => new { c.UserId, c.Name })
                   .IsUnique(); // 同一用户下分类名唯一

            builder.HasOne(c => c.User)
                   .WithMany(u => u.Categories)
                   .HasForeignKey(c => c.UserId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
