using ChatAppNETCore.Models;
using Microsoft.EntityFrameworkCore;

namespace ChatAppNETCore.Models
{
    public class ChatAppContext : DbContext
    {
        public ChatAppContext(DbContextOptions<ChatAppContext> options) : base(options) { }

        public DbSet<C_User> C_Users { get; set; }
        public DbSet<C_Chat> C_Chats { get; set; }
        public DbSet<C_Message> C_Messages { get; set; }
        public DbSet<C_Notification> C_Notification { get; set; }

    }
}
