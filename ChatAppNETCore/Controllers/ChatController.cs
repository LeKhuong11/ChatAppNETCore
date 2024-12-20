﻿using ChatAppNETCore.Services;
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
            var myId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var chats = await _chatService.GetChatsByUserId(myId);
            var groups = await _chatService.getGroupByUserId(myId);
            var users = await _userService.GetAllUserWithOutCurrentUser(myId);

            ViewBag.Chats = chats;
            ViewBag.Groups = groups;
            ViewBag.Users = users;

            return View();
        }
    }
}
