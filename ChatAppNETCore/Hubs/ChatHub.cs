using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using ChatAppNETCore.Models;
using ChatAppNETCore.Hubs.Models;

namespace ChatAppNETCore.Hubs
{
    public class ChatHub : Hub
    {
        private readonly ChatAppContext _context;
        private static ConcurrentDictionary<string, OnlineUser> _onlineUsers = new ConcurrentDictionary<string, OnlineUser>();
        private static Dictionary<string, HashSet<string>> UserRooms = new();

        public ChatHub(ChatAppContext context)
        {
            _context = context;
        }

        public override async Task<Task> OnConnectedAsync()
        {
            string userId = Context.UserIdentifier.ToUpper();
            
            await this.AddUserToOnlineList(userId);

            return base.OnConnectedAsync();
        }

        public override async Task<Task> OnDisconnectedAsync(System.Exception exception)
        {
            string userId = Context.UserIdentifier.ToUpper();

            await this.RemoveUserFromOnlineList(userId);

            return base.OnDisconnectedAsync(exception);
        }

        public async Task AddUserToOnlineList(string userId)
        {
            string connectionId = Context.ConnectionId;
            var isUserAvailable = _onlineUsers.Values.Where(u => u.UserId == userId);

            if (!isUserAvailable.Any())
            {
                OnlineUser user = new OnlineUser(connectionId, userId);

                // Store userId and connectionId into the dictionary
                _onlineUsers.TryAdd(connectionId, user);
            }
            else
            {
                // Update connectionId
                var existingUser = isUserAvailable.FirstOrDefault();
                if (existingUser != null)
                {
                    _onlineUsers.TryRemove(existingUser.ConnectionId, out _); // Remove old user connection
                    existingUser.ConnectionId = connectionId; // Update with new connectionId
                    _onlineUsers.TryAdd(connectionId, existingUser); // Add updated user back
                }
            }

            await Clients.All.SendAsync("userConnection", _onlineUsers.ToList());
        }

        public async Task RemoveUserFromOnlineList(string userId)
        {
            string connectionId = Context.ConnectionId;
            var isUserAvailable = _onlineUsers.Values.Where(u => u.UserId == userId);

            var existingUser = isUserAvailable.FirstOrDefault();
            if (existingUser != null)
            {
                _onlineUsers.TryRemove(existingUser.ConnectionId, out _);
                await Clients.All.SendAsync("userDisconnect", existingUser.UserId);
            }

        }
        
        public async Task SendMessage(string room, string message, string toUserId)
        {
            string userId = Context.UserIdentifier.ToUpper();
            string userName = Context.User.Identity.Name;

            C_Message chatMessage = new C_Message
            {
                ChatId = room,
                SenderId = userId,
                Content = message,
                CreatedAt = DateTime.Now,
            };

            _context.C_Messages.Add(chatMessage);
            await _context.SaveChangesAsync();
            await Clients.Group(room).SendAsync("ReceiveMessage", chatMessage);
          
            await this.SendNotification(chatMessage, userName, toUserId, room);
        }

        public async Task JoinRoom(string room)
        {
            string userId = Context.UserIdentifier.ToUpper();
            string userName = Context.User.Identity.Name;


            if (!UserRooms.ContainsKey(userId))
            {
                UserRooms[userId] = new HashSet<string>();
            }

            foreach (var oldRoom in UserRooms[userId])
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, oldRoom);
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, room);
            UserRooms[userId].Clear(); // Remove old room list
            UserRooms[userId].Add(room); // Add new room to list

            
            await Clients.Group(room).SendAsync("JoinRoomMessage", userName, userId.ToUpper());
        }
            
        public async Task LeaveRoom(string room)
        {
            string userName = Context.User.Identity.Name;

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, room);
            await Clients.Group(room).SendAsync("LeaveRoomMessage", userName);
        }

        public async Task SendNotification(C_Message message, string senderName, string toUserId, string room)
        {
            C_Notification notification = new C_Notification
            {
                ReceiveId = toUserId,
                SenderId = message.SenderId,
                MessageId = message.Id,
            };

            _context.C_Notification.Add(notification);
            await _context.SaveChangesAsync();

            OnlineUser? isUserAvailable = _onlineUsers.Values.FirstOrDefault(u => u.UserId == toUserId);

            if (isUserAvailable != null)
            {
                await Clients.Client(isUserAvailable.ConnectionId).SendAsync("NotificationMessage", notification, message, senderName);
            }
        }
    }
}