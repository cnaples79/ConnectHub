using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using ConnectHub.API.Services;
using ConnectHub.Shared.DTOs;
using System.Security.Claims;

namespace ConnectHub.API.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly ChatService _chatService;
        private static readonly Dictionary<string, string> _userConnections = new();

        public ChatHub(ChatService chatService)
        {
            _chatService = chatService;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                _userConnections[userId] = Context.ConnectionId;
                await Groups.AddToGroupAsync(Context.ConnectionId, $"User_{userId}");
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                _userConnections.Remove(userId);
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"User_{userId}");
            }
            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(SendMessageDto messageDto)
        {
            var senderId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(senderId) || !int.TryParse(senderId, out int senderIdInt))
            {
                throw new HubException("User not authenticated");
            }

            var message = await _chatService.SendMessageAsync(messageDto, senderIdInt);

            // Send to sender's group
            await Clients.Group($"User_{senderId}").SendAsync("ReceiveMessage", message);

            // Send to receiver's group
            await Clients.Group($"User_{messageDto.ReceiverId}").SendAsync("ReceiveMessage", message);
        }

        public async Task MarkMessagesAsRead(int otherUserId)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int userIdInt))
            {
                throw new HubException("User not authenticated");
            }

            await _chatService.MarkMessagesAsReadAsync(userIdInt, otherUserId);

            // Notify the other user that messages have been read
            await Clients.Group($"User_{otherUserId}").SendAsync("MessagesRead", userIdInt);
        }

        public async Task JoinChat(int otherUserId)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                throw new HubException("User not authenticated");
            }

            // Add user to a chat-specific group
            var chatGroup = GetChatGroupName(userId, otherUserId.ToString());
            await Groups.AddToGroupAsync(Context.ConnectionId, chatGroup);
        }

        public async Task LeaveChat(int otherUserId)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                throw new HubException("User not authenticated");
            }

            // Remove user from chat-specific group
            var chatGroup = GetChatGroupName(userId, otherUserId.ToString());
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, chatGroup);
        }

        private static string GetChatGroupName(string user1Id, string user2Id)
        {
            var userIds = new[] { user1Id, user2Id }.OrderBy(id => id);
            return $"Chat_{userIds.First()}_{userIds.Last()}";
        }
    }
}
