using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using app.Dtos.Chat;
using app.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace app.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly IChatRepository _chatRepo;
        private static readonly Dictionary<string, string> _onlineUsers = new();
        // key = userId, value = connectionId

        public ChatHub(IChatRepository chatRepo)
        {
            _chatRepo = chatRepo;
        }

        private string GetCurrentUserId()
        {
            return Context.User?.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new HubException("User is not authenticated.");
        }


        // ── Connect ───────────────────────────────────────────────
        public override async Task OnConnectedAsync()
        {
            var userId = GetCurrentUserId();
            _onlineUsers[userId] = Context.ConnectionId;

            await Clients.Others.SendAsync("UserOnline", userId);
            await Clients.Caller.SendAsync("OnlineUsers", _onlineUsers.Keys.ToList());

            await base.OnConnectedAsync();
        }

        // ── Disconnect ────────────────────────────────────────────
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = GetCurrentUserId();
            _onlineUsers.Remove(userId);

            await Clients.Others.SendAsync("UserOffline", userId);

            await base.OnDisconnectedAsync(exception);
        }

        // ── Send Direct Message ───────────────────────────────────
        public async Task SendDirectMessage(SendDirectMessageDto dto)
        {
            var senderIdString = GetCurrentUserId();
            var senderId = Guid.Parse(senderIdString);

            var message = await _chatRepo.SendDirectMessageAsync(dto, senderId);

            var recipientId = dto.RecipientId.ToString();
            if (_onlineUsers.TryGetValue(recipientId, out var recipientConnectionId))
            {
                await Clients.Client(recipientConnectionId)
                    .SendAsync("ReceiveDirectMessage", message);
            }

            await Clients.Caller.SendAsync("ReceiveDirectMessage", message);
        }

        // ── Send Group Message ────────────────────────────────────
        public async Task SendGroupMessage(Guid groupId, SendGroupMessageDto dto)
        {
            var senderId = Guid.Parse(GetCurrentUserId());

            var message = await _chatRepo.SendGroupMessageAsync(dto, groupId, senderId);

            await Clients.Group(groupId.ToString())
                .SendAsync("ReceiveGroupMessage", message);
        }

        // ── Join Group Room ───────────────────────────────────────
        public async Task JoinGroup(string groupId)
        {
            var userId = GetCurrentUserId();

            await Groups.AddToGroupAsync(Context.ConnectionId, groupId);
            await Clients.Group(groupId).SendAsync("UserJoinedGroup", new
            {
                UserId = userId,
                GroupId = groupId
            });
        }

        // ── Leave Group Room ──────────────────────────────────────
        public async Task LeaveGroup(string groupId)
        {
            var userId = GetCurrentUserId();                   // 👈 string

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupId);
            await Clients.Group(groupId).SendAsync("UserLeftGroup", new
            {
                UserId = userId,
                GroupId = groupId
            });
        }

        // ── Typing Indicator ──────────────────────────────────────
        public async Task TypingDirect(string recipientId)
        {
            var userId = GetCurrentUserId();

            if (_onlineUsers.TryGetValue(recipientId, out var connectionId))
            {
                await Clients.Client(connectionId)
                    .SendAsync("UserTyping", userId);
            }
        }

        public async Task TypingGroup(string groupId)
        {
            var userId = GetCurrentUserId();

            await Clients.OthersInGroup(groupId)
                .SendAsync("UserTypingInGroup", new
                {
                    UserId = userId,
                    GroupId = groupId
                });
        }

        // ── Mark As Read ──────────────────────────────────────────
        public async Task MarkAsRead(string senderId)
        {
            var userId = GetCurrentUserId();

            if (_onlineUsers.TryGetValue(senderId, out var connectionId))
            {
                await Clients.Client(connectionId)
                    .SendAsync("MessageRead", userId);
            }
        }
    }
}