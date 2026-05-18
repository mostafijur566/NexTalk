using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using app.Dtos.Chat;

namespace app.Interface
{
    public interface IChatRepository
    {
        // Direct
        Task<MessageResponseDto> SendDirectMessageAsync(SendDirectMessageDto dto, Guid senderId);
        Task<List<MessageResponseDto>> GetDirectMessagesAsync(Guid userId, Guid otherUserId, int page = 1);

        // Group
        Task<MessageResponseDto> SendGroupMessageAsync(SendGroupMessageDto dto, Guid groupId, Guid senderId);
        Task<List<MessageResponseDto>> GetGroupMessagesAsync(Guid groupId, int page = 1);

        // Conversations list
        Task<List<ConversationResponseDto>> GetConversationsAsync(Guid userId);
    }
}