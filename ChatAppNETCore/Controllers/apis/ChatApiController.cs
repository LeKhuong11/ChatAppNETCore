using ChatAppNETCore.Models;
using ChatAppNETCore.Services;
using ChatAppNETCore.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ChatAppNETCore.Controllers.apis
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ChatApiController : ControllerBase
    {
        private readonly ChatAppContext _context;
        private readonly UserService _userService;

        public ChatApiController(ChatAppContext context, UserService userService)
        {
            _context = context;
            _userService = userService;
        }


        [HttpPost("CreateGroup")]
        public async Task<IActionResult> CreateGroup(string groupName, List<string> Members)
        {
            var chat = new C_Chat
            {
                GroupName = groupName,
                Members = Members,
                IsGroup = true,
                CreatedAt = DateTime.Now
            };


            _context.C_Chats.Add(chat);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Chat = chat,
                Messages = (List<string>)null,
            });
        }



        [HttpGet("GetMessages/{chatId}")]
        public async Task<IActionResult> GetMessagesByChatId(int chatId)
        {
            var messages = await Task.Run(() => _context.C_Messages
                    .Where(message => message.ChatId == chatId.ToString())
                    .OrderBy(message => message.CreatedAt)
                    .ToList());

            return Ok(messages);
        }


        [HttpPost("FindOrCreateChatRoom")]
        public async Task<IActionResult> FindOrCreateChatRoom([FromBody] FindChatsRequest request)
        {
            string currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Tìm phòng chat đã tồn tại giữa current user và userId
            C_Chat existingChat = await Task.Run(() => _context.C_Chats.FirstOrDefault(chat => chat.Members.Contains(currentUserId) && chat.Members.Contains(request.UserId)));

            if (existingChat != null)
            {
                var messages = await Task.Run(() => _context.C_Messages
                    .Where(message => message.ChatId == existingChat.Id.ToString())
                    .OrderBy(message => message.CreatedAt)
                    .ToList());
                
                return Ok(new
                {
                    ChatRoom = existingChat,
                    Messages = messages
                });
            }

            // Nếu chưa có phòng chat, tạo phòng chat mới
            C_Chat newChatRoom = new C_Chat
            {
                Members = new List<string> { currentUserId.ToUpper(), request.UserId.ToUpper() },
                IsGroup = false,
                CreatedAt = DateTime.Now
            };

            _context.C_Chats.Add(newChatRoom);
            await _context.SaveChangesAsync();
                
            return Ok(new
            {
                ChatRoom = newChatRoom,
                Messages = (List<string>)null
            });
        }


        [HttpPost("CreateChatRoom")]
        public async Task<IActionResult> CreateChatRoom([FromBody] CreateChatRequest request)
        {
            if (request.UserId == null)
            {
                return BadRequest("Members list cannot be empty.");
            }

            string myId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            C_Chat newChatRoom = new C_Chat
            {
                Members = { request.UserId, myId },
                IsGroup = false,
                CreatedAt = DateTime.Now
            };

            _context.C_Chats.Add(newChatRoom);
            await _context.SaveChangesAsync();

            Guid userId = new Guid(request.UserId);
            C_User user = await _userService.GetUserById(userId);

            return Ok(new ChatListViewModel
            {
                Id = newChatRoom.Id,
                isGroup = newChatRoom.IsGroup,
                Partner = user
            });
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<C_Chat>> GetChatById(int id)
        {
            C_Chat chat = await _context.C_Chats.FindAsync(id);

            if (chat == null)
            {
                return NotFound();
            }

            return chat;
        }
    }

    public class FindChatsRequest
    {
        public string UserId { get; set; }
    }

    public class CreateChatRequest
    {
        public string UserId { get; set; }
    }
}


