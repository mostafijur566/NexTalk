using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace app.Models
{
    public class GroupMember
    {
        public Guid UserId { get; set; }
        public Guid GroupId { get; set; }
        public bool IsAdmin { get; set; }
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public User User { get; set; } = null!;
        public Group Group { get; set; } = null!;
    }
}