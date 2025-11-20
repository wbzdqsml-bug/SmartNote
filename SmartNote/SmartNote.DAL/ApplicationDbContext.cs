using Microsoft.EntityFrameworkCore;
using SmartNote.Domain.Entities;
using SmartNote.DAL.Configurations;

namespace SmartNote.DAL
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Workspace> Workspaces => Set<Workspace>();
        public DbSet<WorkspaceMember> WorkspaceMembers => Set<WorkspaceMember>();
        public DbSet<Note> Notes => Set<Note>();
        public DbSet<WorkspaceInvitation> WorkspaceInvitations => Set<WorkspaceInvitation>();
        public DbSet<UserProfile> UserProfiles => Set<UserProfile>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // 应用所有配置类
            modelBuilder.ApplyConfiguration(new UserConfiguration());
            modelBuilder.ApplyConfiguration(new WorkspaceConfiguration());
            modelBuilder.ApplyConfiguration(new WorkspaceMemberConfiguration());
            modelBuilder.ApplyConfiguration(new NoteConfiguration());
            modelBuilder.ApplyConfiguration(new WorkspaceInvitationConfiguration());
            modelBuilder.ApplyConfiguration(new UserProfileConfiguration());
            base.OnModelCreating(modelBuilder);
        }
    }
}
