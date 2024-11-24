using ConnectHub.API.Data;
using ConnectHub.Shared.DTOs;
using ConnectHub.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace ConnectHub.API.Services
{
    public class ChatService
    {
        private readonly ConnectHubContext _context;

        public ChatService(ConnectHubContext context)
        {
            _context = context;
        }

        public async Task<ChatMessageDto> SendMessageAsync(SendMessageDto messageDto, int senderId)
        {
            var message = new ChatMessage
            {
                SenderId = senderId,
                ReceiverId = messageDto.ReceiverId,
                Content = messageDto.Content,
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };

            _context.ChatMessages.Add(message);
            await _context.SaveChangesAsync();

            return await GetMessageDtoAsync(message.Id);
        }

        public async Task<List<ChatMessageDto>> GetChatHistoryAsync(int userId1, int userId2, int page = 1, int pageSize = 20)
        {
            var messages = await _context.ChatMessages
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .Where(m => 
                    (m.SenderId == userId1 && m.ReceiverId == userId2) ||
                    (m.SenderId == userId2 && m.ReceiverId == userId1))
                .OrderByDescending(m => m.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return messages.Select(MapToChatMessageDto).ToList();
        }

        public async Task<List<ChatThreadDto>> GetChatThreadsAsync(int userId)
        {
            var latestMessages = await _context.ChatMessages
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .Where(m => m.SenderId == userId || m.ReceiverId == userId)
                .GroupBy(m => m.SenderId == userId ? m.ReceiverId : m.SenderId)
                .Select(g => new
                {
                    OtherUserId = g.Key,
                    LastMessage = g.OrderByDescending(m => m.CreatedAt).First(),
                    UnreadCount = g.Count(m => !m.IsRead && m.ReceiverId == userId)
                })
                .ToListAsync();

            var threads = new List<ChatThreadDto>();
            foreach (var msg in latestMessages)
            {
                var otherUser = await _context.Users.FindAsync(msg.OtherUserId);
                if (otherUser != null)
                {
                    threads.Add(new ChatThreadDto
                    {
                        OtherUser = MapToUserDto(otherUser),
                        LastMessage = MapToChatMessageDto(msg.LastMessage),
                        UnreadCount = msg.UnreadCount
                    });
                }
            }

            return threads.OrderByDescending(t => t.LastMessage.CreatedAt).ToList();
        }

        public async Task MarkMessagesAsReadAsync(int userId, int otherUserId)
        {
            var unreadMessages = await _context.ChatMessages
                .Where(m => m.ReceiverId == userId && m.SenderId == otherUserId && !m.IsRead)
                .ToListAsync();

            foreach (var message in unreadMessages)
            {
                message.IsRead = true;
            }

            await _context.SaveChangesAsync();
        }

        private async Task<ChatMessageDto> GetMessageDtoAsync(int messageId)
        {
            var message = await _context.ChatMessages
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .FirstOrDefaultAsync(m => m.Id == messageId);

            return message != null ? MapToChatMessageDto(message) : null;
        }

        private static ChatMessageDto MapToChatMessageDto(ChatMessage message)
        {
            return new ChatMessageDto
            {
                Id = message.Id,
                Sender = MapToUserDto(message.Sender),
                Receiver = MapToUserDto(message.Receiver),
                Content = message.Content,
                CreatedAt = message.CreatedAt,
                IsRead = message.IsRead
            };
        }

        private static UserDto MapToUserDto(User user)
        {
            return new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Bio = user.Bio,
                ProfileImageUrl = user.ProfileImageUrl,
                CreatedAt = user.CreatedAt,
                LastActive = user.LastActive,
                FollowersCount = user.Followers?.Count ?? 0,
                FollowingCount = user.Following?.Count ?? 0
            };
        }
    }
}
