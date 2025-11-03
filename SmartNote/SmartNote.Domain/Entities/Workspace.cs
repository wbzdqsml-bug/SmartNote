using SmartNote.Domain.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartNote.Domain.Entities
{
    public class Workspace
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public WorkspaceType Type { get; set; }
        public int OwnerUserId { get; set; }
        public DateTime CreateTime { get; set; } = DateTime.UtcNow;
    }
}
