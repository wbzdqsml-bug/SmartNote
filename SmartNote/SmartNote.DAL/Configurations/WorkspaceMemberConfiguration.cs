using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartNote.Domain.Entities;

namespace SmartNote.DAL.Configurations
{
    public class WorkspaceMemberConfiguration : IEntityTypeConfiguration<WorkspaceMember>
    {
        public void Configure(EntityTypeBuilder<WorkspaceMember> builder)
        {
            builder.ToTable("WorkspaceMembers");

            builder.HasKey(m => new { m.WorkspaceId, m.UserId });

            builder.Property(m => m.Role)
                   .HasConversion<int>()
                   .IsRequired();

            builder.Property(m => m.JoinTime)
                   .HasDefaultValueSql("GETUTCDATE()");

            builder.HasOne<Workspace>()
                   .WithMany()
                   .HasForeignKey(m => m.WorkspaceId)
                   .OnDelete(DeleteBehavior.Restrict); // ✅ 改这里

            builder.HasOne<User>()
                   .WithMany()
                   .HasForeignKey(m => m.UserId)
                   .OnDelete(DeleteBehavior.Restrict); // ✅ 改这里
        }
    }
}
