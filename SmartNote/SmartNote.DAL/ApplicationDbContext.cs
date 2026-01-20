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
        public DbSet<Category> Categories { get; set; } = null!;
        public DbSet<Tag> Tags { get; set; } = null!;
        public DbSet<NoteTag> NoteTags { get; set; } = null!;

        public DbSet<NoteActivityLog> NoteActivityLogs => Set<NoteActivityLog>();
        public DbSet<Friendship> Friendships => Set<Friendship>();
        public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();
        public DbSet<NoteAttachment> NoteAttachments => Set<NoteAttachment>();
        public DbSet<PublicContent> PublicContents => Set<PublicContent>();
        public DbSet<PublicContentStats> PublicContentStats => Set<PublicContentStats>();
        public DbSet<PublicComment> PublicComments => Set<PublicComment>();
        public DbSet<PublicContentReaction> PublicContentReactions => Set<PublicContentReaction>();
        public DbSet<TaskItem> TaskItems => Set<TaskItem>();
        public DbSet<TaskLog> TaskLogs => Set<TaskLog>();


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // 应用所有配置类
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
            base.OnModelCreating(modelBuilder);
        }
    }
}
