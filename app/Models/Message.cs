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
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
    public bool IsRead { get; set; }

    // Sender
    public Guid SenderId { get; set; }
    public User Sender { get; set; } = null!;

    // Direct message (null if group)
    public Guid? RecipientId { get; set; }
    public User? Recipient { get; set; }

    // Group message (null if direct)
    public Guid? GroupId { get; set; }
    public Group? Group { get; set; }
    }
}