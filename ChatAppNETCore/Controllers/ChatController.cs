using ChatAppNETCore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

namespace ChatAppNETCore.Controllers
{
    [Authorize]
    public class ChatController : Controller
    {
        private readonly ChatService _chatService;
        private readonly UserService _userService;

        public ChatController(ChatService chatService, UserService userService) 
        { 
           _chatService = chatService;
           _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var chats = await _chatService.GetChatsByUserId(userId);
            var users = await _userService.getAllUserWithOutCurrentUser(userId);


            ViewBag.Chats = chats;
            ViewBag.Users = users;

            return View();
        }
    }
}
