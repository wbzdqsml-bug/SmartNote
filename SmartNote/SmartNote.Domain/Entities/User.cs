using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartNote.Domain.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = default!;
        public string PasswordHash { get; set; } = default!;
        public DateTime CreateTime { get; set; } = DateTime.UtcNow;
        public ICollection<WorkspaceMember> WorkspaceMemberships { get; set; } = new List<WorkspaceMember>();
        public UserProfile? Profile { get; set; }
        public ICollection<Category> Categories { get; set; } = new List<Category>();
        public ICollection<Tag> Tags { get; set; } = new List<Tag>();

    }
}
