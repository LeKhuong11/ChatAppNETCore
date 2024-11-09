using ChatAppNETCore.Models;
using ChatAppNETCore.ViewModels;
using Microsoft.AspNetCore.Http.HttpResults;
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

        public async Task<List<ChatListViewModel>> GetChatsByUserId(string myId)
        {
            var chats = await _context.C_Chats
                .Where(chat => chat.Members.Contains(myId) && !chat.IsGroup)
                .Select(chat => new ChatListViewModel
                {
                    Id = chat.Id,
                    CreatedAt = chat.CreatedAt,

                    LatestMessage = _context.C_Messages
                        .Where(m => m.ChatId == chat.Id.ToString())
                        .OrderByDescending(m => m.CreatedAt)
                        .FirstOrDefault(),


                    Members = _context.C_Users.Where(u => chat.Members.Contains(u.Id.ToString())).ToList(),

                    Partner = _context.C_Users
                        .FirstOrDefault(u => chat.Members.Contains(u.Id.ToString()) && u.Id.ToString() != myId),

                    MessagesUnRead = _context.C_Messages
                        .Where(m => m.ChatId == chat.Id.ToString() && !m.isRead && m.SenderId != myId)
                        .Count(),
                })
                .ToListAsync();

            return chats;
        }
         
        public async Task<List<GroupListViewModel>> getGroupByUserId(string myId)
        {
            var groups = await _context.C_Chats
                .Where(chat => chat.Members.Contains(myId) && chat.IsGroup)
                .Select(chat => new GroupListViewModel
                {
                    Id = chat.Id,
                    GroupName = chat.GroupName,
                    CreatedAt = chat.CreatedAt,

                    LatestMessage = _context.C_Messages
                        .Where(m => m.ChatId == chat.Id.ToString())
                        .OrderByDescending(m => m.CreatedAt)
                        .FirstOrDefault(),

                    Members = _context.C_Users.Where(u => chat.Members.Contains(u.Id.ToString())).ToList(),
                })
                .ToListAsync();

            return groups;
        }


        public static implicit operator ChatService(UserService u)
        {
            throw new NotImplementedException();
        }
    }
}
