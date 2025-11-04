using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartNote.Domain.Entities;

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

        builder.Property(m => m.CanEdit)
               .HasDefaultValue(false);

        builder.Property(m => m.CanShare)
               .HasDefaultValue(false);

        // ✅ 正确的导航绑定
        builder.HasOne(m => m.Workspace)
               .WithMany(w => w.Members)
               .HasForeignKey(m => m.WorkspaceId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(m => m.User)
         .WithMany(u => u.WorkspaceMemberships)
         .HasForeignKey(m => m.UserId)
         .OnDelete(DeleteBehavior.Restrict);

    }
}
