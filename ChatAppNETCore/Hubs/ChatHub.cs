using ChatAppNETCore.Models;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Security.Claims;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace ChatAppNETCore.Hubs
{
    public class ChatHub : Hub
    {
        private readonly ChatAppContext _context;
        private static ConcurrentDictionary<string, string> _onlineUsers = new ConcurrentDictionary<string, string>();

        public ChatHub(ChatAppContext context)
        {
            _context = context;
        }

        public override Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier.ToUpper();
            var connectionId = Context.ConnectionId;

            Console.WriteLine("connected: ");
            Console.WriteLine(userId);
            Console.WriteLine(connectionId);

            // Store userId and connectionId into the dictionary
            _onlineUsers.TryAdd(userId, connectionId);

            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(System.Exception exception)
        {
            var userId = Context.UserIdentifier.ToUpper();
            var connectionId = Context.ConnectionId;

            Console.WriteLine("removed: ");
            Console.WriteLine(userId);
            Console.WriteLine(connectionId);

            // If the user is disconnected, remove the userId from the dictionary
            _onlineUsers.TryRemove(userId, out _);

            return base.OnDisconnectedAsync(exception);
        }
            
        public async Task SendMessage(string room, string message, string toUserId)
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
            await this.SendNotification(chatMessage, toUserId, room);
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

        public async Task SendNotification(C_Message message, string toUserId,  string room)
        {
            var notification = new C_Notification
            {
                ReceiveId = toUserId,
                SenderId = message.SenderId,
                MessageId = message.Id,
            };

            _context.C_Notification.Add(notification);
            await _context.SaveChangesAsync();

            if (_onlineUsers.TryGetValue(toUserId, out var connectionId))
            {
                await Clients.Client(connectionId).SendAsync("NotificationMessage", notification, message);
            }
        }
    }
}