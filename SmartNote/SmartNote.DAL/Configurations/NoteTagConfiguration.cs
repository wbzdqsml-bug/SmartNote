using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartNote.Domain.Entities;

public class NoteTagConfiguration : IEntityTypeConfiguration<NoteTag>
{
    public void Configure(EntityTypeBuilder<NoteTag> builder)
    {
        builder.ToTable("NoteTags");

        // 复合主键
        builder.HasKey(nt => new { nt.NoteId, nt.TagId });

        // Tag 删除 -> 删除 NoteTag
        builder.HasOne(nt => nt.Tag)
               .WithMany(t => t.NoteTags)
               .HasForeignKey(nt => nt.TagId)
               .OnDelete(DeleteBehavior.Cascade);

        // Note 删除 -> 删除 NoteTag
        builder.HasOne(nt => nt.Note)
               .WithMany(n => n.NoteTags)
               .HasForeignKey(nt => nt.NoteId)
               .OnDelete(DeleteBehavior.Cascade)
               .IsRequired(false);  // ★★★ 最关键的一句
    }
}
