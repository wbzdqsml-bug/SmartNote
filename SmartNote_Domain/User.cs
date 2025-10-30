using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartNote_Domain
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public DateTime CreateTime { get; set; }

        // 导航属性
        // 1-to-1 -> UserProfile
        public virtual UserProfile? UserProfile { get; set; }

        // Many-to-Many -> Workspaces
        public virtual ICollection<WorkspaceMember> WorkspaceMembers { get; set; } = new List<WorkspaceMember>();
    }
}
