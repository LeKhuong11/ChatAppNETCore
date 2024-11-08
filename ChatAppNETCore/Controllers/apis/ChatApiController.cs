using ChatAppNETCore.Models;
using ChatAppNETCore.Services;
using ChatAppNETCore.ViewModels;
using Microsoft.AspNetCore.Authorization;
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
        public async Task<IActionResult> CreateGroup([FromBody] CreateGroupRequest request)
        {
            var chat = new C_Chat
            {
                GroupName = request.GroupName,
                Members = request.Members,
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

        [HttpGet("MarkAsRead/{chatId}")]
        public async Task<IActionResult> MarkAsRead(string chatId)
        {
            string myId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var unReadMessage = await _context.C_Messages
                .Where(m => m.ChatId == chatId && !m.isRead && m.SenderId != myId)
                .ToListAsync();

            unReadMessage.ForEach(message => message.isRead = true);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "All messages marked as read.",
                status = true
            });
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
            Guid userId = new Guid(request.UserId);
            C_User user = await _userService.GetUserById(userId);

            C_Chat existingChat = await Task.Run(() => _context.C_Chats.FirstOrDefault(chat => chat.Members.Contains(myId) && chat.Members.Contains(request.UserId)));

            if(existingChat != null)
            {
                var messages = await Task.Run(() => _context.C_Messages
                    .Where(message => message.ChatId == existingChat.Id.ToString())
                    .OrderBy(message => message.CreatedAt)
                    .ToList());

                return Ok(new
                {
                    ChatRoom = existingChat,
                    Messages = messages,
                    isNewChat = false,
                    Partner = user
                });
            }

            C_Chat newChatRoom = new C_Chat
            {
                Members = new List<string> { request.UserId.ToUpper(), myId.ToUpper() },
                IsGroup = false,
                CreatedAt = DateTime.Now
            };

            _context.C_Chats.Add(newChatRoom);
            await _context.SaveChangesAsync();

            return Ok(new ChatListViewModel
            {
                Id = newChatRoom.Id,
                isNewChat = true,
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

    public class CreateGroupRequest
    {
        public string GroupName { get; set; }
        public List<string> Members { get; set; }
    }
}


