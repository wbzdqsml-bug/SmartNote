using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartNote.Domain.Entities;

namespace SmartNote.DAL.Configurations
{
    public class TaskLogConfiguration : IEntityTypeConfiguration<TaskLog>
    {
        public void Configure(EntityTypeBuilder<TaskLog> builder)
        {
            builder.ToTable("TaskLogs");

            builder.HasOne(log => log.TaskItem)
                .WithMany(task => task.Logs)
                .HasForeignKey(log => log.TaskItemId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(log => log.ActorUser)
                .WithMany(user => user.TaskLogs)
                .HasForeignKey(log => log.ActorUserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
