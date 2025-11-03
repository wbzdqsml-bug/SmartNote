using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartNote.Domain.Entities;

namespace SmartNote.DAL.Configurations
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
                   .HasConversion<int>()
                   .IsRequired();

            builder.Property(w => w.CreateTime)
                   .HasDefaultValueSql("GETUTCDATE()");

            builder.HasIndex(w => w.OwnerUserId);

            // 关系：Workspace -> Owner(User)
            builder.HasOne<User>()
                   .WithMany()
                   .HasForeignKey(w => w.OwnerUserId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
