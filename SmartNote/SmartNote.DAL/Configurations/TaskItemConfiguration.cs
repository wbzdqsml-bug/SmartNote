using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartNote.Domain.Entities;

namespace SmartNote.DAL.Configurations
{
    public class TaskItemConfiguration : IEntityTypeConfiguration<TaskItem>
    {
        public void Configure(EntityTypeBuilder<TaskItem> builder)
        {
            builder.ToTable("TaskItems");

            builder.HasOne(task => task.Workspace)
                .WithMany()
                .HasForeignKey(task => task.WorkspaceId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(task => task.Note)
                .WithMany()
                .HasForeignKey(task => task.NoteId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(task => task.OwnerUser)
                .WithMany(user => user.TaskItems)
                .HasForeignKey(task => task.OwnerUserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
