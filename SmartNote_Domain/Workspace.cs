using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartNote_Domain
{
    public class Workspace
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // "Personal" 或 "Team"
        public int OwnerUserId { get; set; } // 创建者
        public DateTime CreateTime { get; set; }

        // 导航属性
        // Many-to-Many -> Users
        public virtual ICollection<WorkspaceMember> Members { get; set; } = new List<WorkspaceMember>();

        // 1-to-Many -> Notes
        public virtual ICollection<Note> Notes { get; set; } = new List<Note>();
    }
}
