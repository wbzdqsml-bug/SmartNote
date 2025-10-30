using Microsoft.EntityFrameworkCore;
using SmartNote_Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SmartNote_DAL
{
    public class ApplicationDbContext : DbContext
    {
        // 构造函数，用于接收来自 Program.cs 的配置
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // 定义所有的数据库表 (DbSet)
        public DbSet<User> Users { get; set; }
        public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<Workspace> Workspaces { get; set; }
        public DbSet<WorkspaceMember> WorkspaceMembers { get; set; }
        public DbSet<Note> Notes { get; set; }

        // (迭代 5 将在这里添加 Files, Tags, AnalysisResults, Statistics 等)


        // 重写 OnModelCreating 方法，告诉 EF Core 自动加载所有配置
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 自动扫描并应用 SmartNote.DAL 程序集中
            // 所有实现了 IEntityTypeConfiguration<TEntity> 接口的配置类
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }
    }
}