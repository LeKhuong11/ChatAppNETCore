using ChatAppNETCore.Models;
using ChatAppNETCore.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace ChatAppNETCore.Services
{
    public interface IChatService
    {
        List<C_Chat> GetMessagesByUserId(int userId);
    }


    public class ChatService
    {
        private readonly ChatAppContext _context;

        public ChatService(ChatAppContext context)
        {
            _context = context;
        }

        public async Task<List<ChatListViewModel>> GetChatsByUserId(string userId)
        {
            var chats = await _context.C_Chats
                .Where(chat => chat.Members.Contains(userId))
                .Select(chat => new ChatListViewModel
                {
                    Id = chat.Id,
                    CreatedAt = chat.CreatedAt,
                    isGroup = chat.IsGroup,

                    Message = _context.C_Messages
                        .Where(m => m.ChatId == chat.Id.ToString())
                        .OrderByDescending(m => m.CreatedAt)
                        .FirstOrDefault(),

                    Members = chat.IsGroup ? _context.C_Users
                        .Where(u => chat.Members.Contains(u.Id.ToString())).ToList() : null,

                    Partner = !chat.IsGroup ? _context.C_Users
                        .FirstOrDefault(u => chat.Members.Contains(u.Id.ToString()) && u.Id.ToString() != userId) : null
                })
                .ToListAsync();

            return chats;
        }

        public static implicit operator ChatService(UserService u)
        {
            throw new NotImplementedException();
        }
    }
}
