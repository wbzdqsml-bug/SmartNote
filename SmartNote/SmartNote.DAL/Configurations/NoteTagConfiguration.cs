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
    public class NoteTagConfiguration : IEntityTypeConfiguration<NoteTag>
    {
        public void Configure(EntityTypeBuilder<NoteTag> builder)
        {
            builder.ToTable("NoteTags");

            builder.HasKey(nt => new { nt.NoteId, nt.TagId }); // 复合主键


            builder.HasOne(nt => nt.Tag)
                   .WithMany(t => t.NoteTags)
                   .HasForeignKey(nt => nt.TagId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(nt => nt.Note)
                   .WithMany(n => n.NoteTags)
                   .HasForeignKey(nt => nt.NoteId)
                   .OnDelete(DeleteBehavior.Cascade)
                   .IsRequired(false);  // ⭐ 必须改为 false

        }
    }
}
