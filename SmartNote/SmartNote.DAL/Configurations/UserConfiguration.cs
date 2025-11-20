using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartNote.Domain.Entities;

namespace SmartNote.DAL.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("Users");

            builder.HasKey(u => u.Id);

            builder.Property(u => u.Username)
                   .IsRequired()
                   .HasMaxLength(32);

            builder.Property(u => u.PasswordHash)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.HasIndex(u => u.Username)
                   .IsUnique();

            builder.Property(u => u.CreateTime)
                   .HasDefaultValueSql("GETUTCDATE()");

            builder.HasOne(u => u.Profile)
               .WithOne(p => p.User)
               .HasForeignKey<UserProfile>(p => p.UserId)
               .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
