using System.ComponentModel.DataAnnotations;

namespace ChatAppNETCore.Hubs.Models
{
    public class OnlineUser
    {
        [Required]
        public string ConnectionId { get; set; }

        [Required]
        public string UserId { get; set; }

        public DateTime ConnectedAt { get; set; }


        public OnlineUser(string connectionId, string userId)
        {
            ConnectionId = connectionId;
            UserId = userId;
            ConnectedAt = DateTime.UtcNow; 
        }
    }
}
