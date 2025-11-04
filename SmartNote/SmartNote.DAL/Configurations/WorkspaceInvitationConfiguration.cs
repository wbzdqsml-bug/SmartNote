using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartNote.Domain.Entities;

namespace SmartNote.DAL.Configurations
{
    public class WorkspaceInvitationConfiguration : IEntityTypeConfiguration<WorkspaceInvitation>
    {
        public void Configure(EntityTypeBuilder<WorkspaceInvitation> builder)
        {
            builder.ToTable("WorkspaceInvitations");

            builder.HasKey(i => i.Id);

            builder.Property(i => i.Status)
                   .HasConversion<int>()
                   .IsRequired();

            builder.Property(i => i.CanEdit).HasDefaultValue(false);
            builder.Property(i => i.CanShare).HasDefaultValue(false);

            builder.Property(i => i.CreatedTime)
                   .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(i => i.Message)
                   .HasMaxLength(500);

            builder.HasIndex(i => new { i.InviteeUserId, i.Status });

            // FK：Workspace（删除工作区时，邀请自动删除）
            builder.HasOne(i => i.Workspace)
                   .WithMany()
                   .HasForeignKey(i => i.WorkspaceId)
                   .OnDelete(DeleteBehavior.Cascade);

            // FK：Inviter（限制删除用户时不级联，避免误删）
            builder.HasOne(i => i.InviterUser)
                   .WithMany()
                   .HasForeignKey(i => i.InviterUserId)
                   .OnDelete(DeleteBehavior.Restrict);

            // FK：Invitee（限制删除用户时不级联，避免误删）
            builder.HasOne(i => i.InviteeUser)
                   .WithMany()
                   .HasForeignKey(i => i.InviteeUserId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
