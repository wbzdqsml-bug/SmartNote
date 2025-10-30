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
    public class WorkspaceConfiguration : IEntityTypeConfiguration<Workspace>
    {
        public void Configure(EntityTypeBuilder<Workspace> builder)
        {
            builder.ToTable("Workspaces");
            builder.HasKey(w => w.Id);

            builder.Property(w => w.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(w => w.Type)
                .IsRequired()
                .HasMaxLength(20); // "Personal", "Team"

            builder.Property(w => w.CreateTime)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            // 配置 1-to-Many (Workspace -> Notes)
            builder.HasMany(w => w.Notes)
                .WithOne(n => n.Workspace)
                .HasForeignKey(n => n.WorkspaceId)
                .OnDelete(DeleteBehavior.Cascade); // 删除工作区时，级联删除所有笔记
        }
    }
}