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
    public class UserProfileConfiguration : IEntityTypeConfiguration<UserProfile>
    {
        public void Configure(EntityTypeBuilder<UserProfile> builder)
        {
            builder.ToTable("UserProfiles");
            builder.HasKey(up => up.Id);

            builder.Property(up => up.Nickname)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(up => up.AvatarUrl).HasMaxLength(255);
            builder.Property(up => up.Bio).HasMaxLength(500);

            builder.Property(up => up.LastUpdateTime)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            // 配置 1-to-1 关系 (User -> UserProfile)
            // (注意：外键在 UserProfile 表上)
            builder.HasOne(up => up.User)
                .WithOne(u => u.UserProfile)
                .HasForeignKey<UserProfile>(up => up.UserId) // UserId 是外键
                .OnDelete(DeleteBehavior.Cascade); // 删除 User 时，级联删除 UserProfile

            builder.HasIndex(up => up.UserId).IsUnique(); // 确保外键也是唯一的
        }
    }
}
