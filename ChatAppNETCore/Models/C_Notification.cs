using System.ComponentModel.DataAnnotations;

namespace ChatAppNETCore.Models
{
    public class C_Notification
    {
        [Key]
        public int Id { get; set; }

        public string ReceiveId { get; set; }

        [Required]
        public string SenderId { get; set; }

        [Required]
        public int MessageId { get; set; }

        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
