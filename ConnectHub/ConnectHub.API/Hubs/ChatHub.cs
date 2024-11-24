using Microsoft.AspNetCore.SignalR;
using ConnectHub.API.Data;
using ConnectHub.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace ConnectHub.API.Hubs
{
    public class ChatHub : Hub
    {
        private readonly ConnectHubContext _context;
        private static readonly Dictionary<string, string> _userConnections = new();

        public ChatHub(ConnectHubContext context)
        {
            _context = context;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst("sub")?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                _userConnections[userId] = Context.ConnectionId;
                await Clients.All.SendAsync("UserOnline", userId);
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = _userConnections.FirstOrDefault(x => x.Value == Context.ConnectionId).Key;
            if (!string.IsNullOrEmpty(userId))
            {
                _userConnections.Remove(userId);
                await Clients.All.SendAsync("UserOffline", userId);
            }
            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendPrivateMessage(string receiverId, string content)
        {
            var senderId = Context.User?.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(senderId))
                throw new HubException("User not authenticated");

            var message = new PrivateMessage
            {
                Id = Guid.NewGuid().ToString(),
                SenderId = senderId,
                ReceiverId = receiverId,
                Content = content,
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };

            _context.PrivateMessages.Add(message);
            await _context.SaveChangesAsync();

            if (_userConnections.TryGetValue(receiverId, out string? connectionId))
            {
                await Clients.Client(connectionId).SendAsync("ReceiveMessage", message);
            }

            await Clients.Caller.SendAsync("MessageSent", message);
        }

        public async Task MarkMessageAsRead(string messageId)
        {
            var userId = Context.User?.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId))
                throw new HubException("User not authenticated");

            var message = await _context.PrivateMessages.FindAsync(messageId);
            if (message != null && message.ReceiverId == userId)
            {
                message.IsRead = true;
                await _context.SaveChangesAsync();
                
                if (_userConnections.TryGetValue(message.SenderId, out string? connectionId))
                {
                    await Clients.Client(connectionId).SendAsync("MessageRead", messageId);
                }
            }
        }

        public async Task GetUnreadMessageCount()
        {
            var userId = Context.User?.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId))
                throw new HubException("User not authenticated");

            var count = await _context.PrivateMessages
                .CountAsync(m => m.ReceiverId == userId && !m.IsRead);

            await Clients.Caller.SendAsync("UnreadMessageCount", count);
        }
    }
}
