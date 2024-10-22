using ChatAppNETCore.Models;

namespace ChatAppNETCore.ViewModels
{
    public class ChatListViewModel
    {
        public int Id { get; set; }

        public bool isGroup { get; set; }

        public C_User Partner { get; set; }

        public List<C_User> Members { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
