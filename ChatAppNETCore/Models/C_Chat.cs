﻿using System.ComponentModel.DataAnnotations;

namespace ChatAppNETCore.Models
{
    public class C_Chat
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public List<string> Members { get; set; }

        [Required]
        public bool IsGroup { get; set; } = false;

        public string? GroupName { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public C_Chat()
        {
            Members = new List<string>();
        }
    }
}
