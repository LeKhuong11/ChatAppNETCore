using System.ComponentModel.DataAnnotations;

namespace ChatAppNETCore.Models
{
    public class C_Message
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string ChatId { get; set; }

        [Required]
        public string SenderId { get; set; }

        [Required]
        public string Content { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
