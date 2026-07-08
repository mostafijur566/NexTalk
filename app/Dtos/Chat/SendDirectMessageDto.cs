using System.ComponentModel.DataAnnotations;

namespace app.Dtos.Chat
{
    public class SendDirectMessageDto
    {
        [Required]
        [StringLength(50, ErrorMessage = "Username cannot exceed 50 characters.")]
        public string RecipientUsername { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Message content cannot exceed 1000 characters.")]
        public string Content { get; set; } = string.Empty;
    }
}