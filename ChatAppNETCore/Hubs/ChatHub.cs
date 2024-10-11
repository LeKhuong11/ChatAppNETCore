using Microsoft.AspNetCore.SignalR;

namespace ChatAppNETCore.Hubs
{
    public class ChatHub : Hub
    {
        public async Task SendMessage(string user, string message)
        {
            await Clients.Group("room").SendAsync("ReceiveMessage", user, message);
        }

        public async Task JoinRoom(string room)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, room);
            await Clients.Group(room).SendAsync("ReceiveMessage", "System", $"{Context.ConnectionId} has joined the room.");
        }
    }
}