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
    public class WorkspaceMemberConfiguration : IEntityTypeConfiguration<WorkspaceMember>
    {
        public void Configure(EntityTypeBuilder<WorkspaceMember> builder)
        {
            builder.ToTable("WorkspaceMembers");

            // 1. 配置复合主键 (WorkspaceId, UserId)
            builder.HasKey(wm => new { wm.WorkspaceId, wm.UserId });

            // 2. 配置 User 和 WorkspaceMember 之间的多对多关系
            builder.HasOne(wm => wm.User)
                .WithMany(u => u.WorkspaceMembers)
                .HasForeignKey(wm => wm.UserId)
                .OnDelete(DeleteBehavior.Cascade); // 删除用户时，也删除他的成员关系

            builder.HasOne(wm => wm.Workspace)
                .WithMany(w => w.Members)
                .HasForeignKey(wm => wm.WorkspaceId)
                .OnDelete(DeleteBehavior.Cascade); // 删除工作区时，也删除所有成员关系

            // 3. 配置 Role 属性
            builder.Property(wm => wm.Role)
                .IsRequired()
                .HasMaxLength(20); // "Owner", "Admin", "Member"

            builder.Property(wm => wm.JoinTime)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");
        }
    }
}