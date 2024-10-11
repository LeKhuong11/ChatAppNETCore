using Microsoft.AspNetCore.SignalR;

namespace ChatAppNETCore.Hubs
{
    public class ChatHub : Hub
    {
        public async Task SendMessage(string room, string message)
        {
            await Clients.Group(room).SendAsync("ReceiveMessage", message);
        }

        public async Task JoinRoom(string room, string? user)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, room);
            //await Clients.Group(room).SendAsync("ReceiveMessage", user, $"{user} has joined the room {room}");
        }
    }
}