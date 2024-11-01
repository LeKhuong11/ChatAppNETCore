using ChatAppNETCore.Models;
using System.ComponentModel.DataAnnotations;

namespace ChatAppNETCore.ViewModels
{
    public class GroupListViewModel
    {
        public int Id { get; set; }

        [Required]
        public string GroupName { get; set; }

        public bool isNewChat { get; set; }

        public C_Message? LatestMessage { get; set; }

        public List<C_User>? Members { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
