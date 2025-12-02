using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartNote.Domain.Entities;

namespace SmartNote.DAL.Configurations
{
    public class NoteActivityLogConfiguration : IEntityTypeConfiguration<NoteActivityLog>
    {
        public void Configure(EntityTypeBuilder<NoteActivityLog> builder)
        {
            builder.ToTable("NoteActivityLogs");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Action)
                   .IsRequired()
                   .HasMaxLength(20);

            builder.Property(x => x.Time)
                   .IsRequired();

            // User（一）- 日志（多）
            builder.HasOne(x => x.User)
                   .WithMany(u => u.NoteActivityLogs)
                   .HasForeignKey(x => x.UserId)
                   .OnDelete(DeleteBehavior.Cascade);

            // Note（一）- 日志（多），注意设成可选，避免和 Note 的全局过滤冲突警告
            builder.HasOne(x => x.Note)
                   .WithMany()
                   .HasForeignKey(x => x.NoteId)
                   .OnDelete(DeleteBehavior.Cascade)
                   .IsRequired(false);
        }
    }
}
