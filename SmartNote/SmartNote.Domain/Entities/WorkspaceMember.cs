using SmartNote.Domain.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartNote.Domain.Entities
{
    public class WorkspaceMember
    {
        public int WorkspaceId { get; set; }
        public int UserId { get; set; }
        public WorkspaceRole Role { get; set; } = WorkspaceRole.Member;
        public DateTime JoinTime { get; set; } = DateTime.UtcNow;
    }
}
