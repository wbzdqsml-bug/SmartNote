using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartNote_Domain
{
    public class WorkspaceMember
    {
        // 复合主键 (将在 Fluent API 中配置)
        public int WorkspaceId { get; set; }
        public virtual Workspace Workspace { get; set; } = null!;

        public int UserId { get; set; }
        public virtual User User { get; set; } = null!;

        public string Role { get; set; } = string.Empty; // "Owner", "Admin", "Member"
        public DateTime JoinTime { get; set; }
    }
}
