using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace app.Models
{
    public class Message
    {
        public Guid Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }
        public Guid SenderId { get; set; }
        public User Sender { get; set; }
        public Guid? ConversationId { get; set; }   // 1-on-1
        public Guid? GroupId { get; set; }           // group
    }
}