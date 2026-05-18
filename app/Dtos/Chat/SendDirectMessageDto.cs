using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace app.Dtos.Chat
{
    public class SendDirectMessageDto
    {
        [Required]
        public Guid RecipientId { get; set; }
        
        [Required]
        [StringLength(1000, ErrorMessage = "Message content cannot exceed 1000 characters.")]
        public string Content { get; set; } = string.Empty;
    }
}