using ChatAppNETCore.Models;

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

        public List<C_Chat> GetChatsByUserId(string userId)
        {
            var chats = _context.C_Chats
                .Where(chat => chat.Members.Contains(userId))
                .ToList();

            return chats;
        }

        public static implicit operator ChatService(UserService v)
        {
            throw new NotImplementedException();
        }
    }
}
