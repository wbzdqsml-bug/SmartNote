using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartNote.Domain.Entities;

namespace SmartNote.DAL.Configurations
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

            builder.Property(n => n.Type)
                   .HasConversion<int>();

            builder.Property(n => n.ContentJson)
                   .HasColumnType("nvarchar(max)")
                   .IsRequired();

            builder.Property(n => n.CreateTime)
                   .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(n => n.LastUpdateTime)
                   .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(n => n.IsDeleted)
                   .HasDefaultValue(false);

            builder.HasIndex(n => new { n.WorkspaceId, n.LastUpdateTime });

            // 软删除全局过滤器
            builder.HasQueryFilter(n => !n.IsDeleted);

            // ================================
            // ⭐ 关键 1：Note → Workspace 禁止级联删除
            // ================================
            builder.HasOne(n => n.Workspace)
                   .WithMany(w => w.Notes)
                   .HasForeignKey(n => n.WorkspaceId)
                   .OnDelete(DeleteBehavior.Restrict);

            // ================================
            // ⭐ 关键 2：Note → Category
            // 删除分类时，Note.CategoryId 置 Null
            // ================================
            builder.HasOne(n => n.Category)
                   .WithMany(c => c.Notes)
                   .HasForeignKey(n => n.CategoryId)
                   .OnDelete(DeleteBehavior.SetNull);

            // ================================
            // ⭐ 关键 3：Note → NoteTags（声明关系）
            // 这是为了模型更清晰，不会额外生成外键
            // ================================
            builder.HasMany(n => n.NoteTags)
                   .WithOne(nt => nt.Note)
                   .HasForeignKey(nt => nt.NoteId);
        }
    }
}
