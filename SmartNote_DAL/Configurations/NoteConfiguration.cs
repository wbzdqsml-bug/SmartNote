using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartNote_Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartNote_DAL.Configurations
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

            // 配置 nvarchar(max)，默认就是可空的 (string?)
            builder.Property(n => n.ContentMd);
            builder.Property(n => n.ContentHtml);

            builder.Property(n => n.CreateTime)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(n => n.LastUpdateTime)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            // (外键关系在 WorkspaceConfiguration 中已配置)
        }
    }
}
