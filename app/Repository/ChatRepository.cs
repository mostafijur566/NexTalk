using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using app.Data;
using app.Dtos.Chat;
using app.Interface;
using app.Models;
using Microsoft.EntityFrameworkCore;

namespace app.Repository
{
    public class ChatRepository : IChatRepository
    {
        private readonly ApplicationDbContext _context;
        private const int PageSize = 20;

        public ChatRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // ── Direct message ────────────────────────────────────────
        public async Task<MessageResponseDto> SendDirectMessageAsync(SendDirectMessageDto dto, Guid senderId)
        {
            var recipient = await _context.Users.FindAsync(dto.RecipientId)
                ?? throw new Exception("Recipient not found.");

            var message = new Message
            {
                Id = Guid.NewGuid(),
                Content = dto.Content,
                SentAt = DateTime.UtcNow,
                SenderId = senderId,
                RecipientId = dto.RecipientId,
                GroupId = null,
                IsRead = false
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            var sender = await _context.Users.FindAsync(senderId);

            return new MessageResponseDto
            {
                Id = message.Id,
                Content = message.Content,
                SentAt = message.SentAt,
                IsRead = message.IsRead,
                SenderId = message.SenderId,
                SenderUsername = sender!.Username,
                RecipientId = message.RecipientId,
                GroupId = null
            };
        }

        public async Task<List<MessageResponseDto>> GetDirectMessagesAsync(Guid userId, Guid otherUserId, int page = 1)
        {
            return await _context.Messages
                .Where(m => m.GroupId == null &&
                    ((m.SenderId == userId && m.RecipientId == otherUserId) ||
                     (m.SenderId == otherUserId && m.RecipientId == userId)))
                .Include(m => m.Sender)
                .OrderByDescending(m => m.SentAt)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .Select(m => new MessageResponseDto
                {
                    Id = m.Id,
                    Content = m.Content,
                    SentAt = m.SentAt,
                    IsRead = m.IsRead,
                    SenderId = m.SenderId,
                    SenderUsername = m.Sender.Username,
                    RecipientId = m.RecipientId,
                    GroupId = null
                })
                .ToListAsync();
        }

        // ── Group message ─────────────────────────────────────────
        public async Task<MessageResponseDto> SendGroupMessageAsync(SendGroupMessageDto dto, Guid groupId, Guid senderId)
        {
            // check sender is a member
            var isMember = await _context.GroupMembers
                .AnyAsync(gm => gm.GroupId == groupId && gm.UserId == senderId);

            if (!isMember)
                throw new Exception("You are not a member of this group.");

            var message = new Message
            {
                Id = Guid.NewGuid(),
                Content = dto.Content,
                SentAt = DateTime.UtcNow,
                SenderId = senderId,
                GroupId = groupId,
                RecipientId = null,
                IsRead = false
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            var sender = await _context.Users.FindAsync(senderId);

            return new MessageResponseDto
            {
                Id = message.Id,
                Content = message.Content,
                SentAt = message.SentAt,
                IsRead = message.IsRead,
                SenderId = message.SenderId,
                SenderUsername = sender!.Username,
                RecipientId = null,
                GroupId = message.GroupId
            };
        }

        public async Task<List<MessageResponseDto>> GetGroupMessagesAsync(Guid groupId, int page = 1)
        {
            // check group exists
            var groupExists = await _context.Groups.AnyAsync(g => g.Id == groupId);
            if (!groupExists)
                throw new Exception("Group not found.");

            return await _context.Messages
                .Where(m => m.GroupId == groupId)
                .Include(m => m.Sender)
                .OrderByDescending(m => m.SentAt)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .Select(m => new MessageResponseDto
                {
                    Id = m.Id,
                    Content = m.Content,
                    SentAt = m.SentAt,
                    IsRead = m.IsRead,
                    SenderId = m.SenderId,
                    SenderUsername = m.Sender.Username,
                    RecipientId = null,
                    GroupId = m.GroupId
                })
                .ToListAsync();
        }

        // ── Conversations list ────────────────────────────────────
        public async Task<List<ConversationResponseDto>> GetConversationsAsync(Guid userId)
        {
            // get latest message per conversation partner
            var conversations = await _context.Messages
                .Where(m => m.GroupId == null &&
                    (m.SenderId == userId || m.RecipientId == userId))
                .Include(m => m.Sender)
                .Include(m => m.Recipient)
                .GroupBy(m => m.SenderId == userId ? m.RecipientId : m.SenderId)
                .Select(g => new ConversationResponseDto
                {
                    UserId = g.Key!.Value,
                    Username = g.First().SenderId == userId
                        ? g.First().Recipient!.Username
                        : g.First().Sender.Username,
                    LastMessage = g.OrderByDescending(m => m.SentAt)
                                   .First().Content,
                    LastMessageAt = g.OrderByDescending(m => m.SentAt)
                                     .First().SentAt,
                    UnreadCount = g.Count(m => m.RecipientId == userId && !m.IsRead)
                })
                .OrderByDescending(c => c.LastMessageAt)
                .ToListAsync();

            return conversations;
        }
    }
}