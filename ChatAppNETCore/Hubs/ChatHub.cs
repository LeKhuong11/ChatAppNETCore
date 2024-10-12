using ChatAppNETCore.Models;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace ChatAppNETCore.Hubs
{
    public class ChatHub : Hub
    {
        private readonly ChatAppContext _context;

        public ChatHub(ChatAppContext context)
        {
            _context = context;
        }


        public async Task SendMessage(string room, string message)
        {
            string userName = Context.User.Identity.Name;
            string userId = Context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var chatMessage = new C_Message
            {
                ChatId = room,
                SenderId = userId.ToUpper(),
                Content = message,
                CreatedAt = DateTime.UtcNow,
            };

            _context.C_Messages.Add(chatMessage);
            await _context.SaveChangesAsync();

            await Clients.Group(room).SendAsync("ReceiveMessage", chatMessage);
        }

        public async Task JoinRoom(string room)
        {
            string userName = Context.User.Identity.Name;
            string userId = Context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            await Groups.AddToGroupAsync(Context.ConnectionId, room);
            await Clients.Group(room).SendAsync("JoinRoomMessage", userName, userId.ToUpper());
        }

        public async Task LeaveRoom(string room)
        {
            string userName = Context.User.Identity.Name;

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, room);
            await Clients.Group(room).SendAsync("LeaveRoomMessage", userName);
        }
    }
}