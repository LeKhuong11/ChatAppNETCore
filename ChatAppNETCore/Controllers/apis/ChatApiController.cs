using ChatAppNETCore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ChatAppNETCore.Controllers.apis
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ChatApiController : ControllerBase
    {
        private readonly ChatAppContext _context;

        public ChatApiController(ChatAppContext context)
        {
            _context = context;
        }

        [HttpGet("GetChat")]
        public async Task<IActionResult> GetChat(int chatId)
        {

            var messages = await Task.Run(() => _context.C_Messages
                .Where(message => message.ChatId == chatId.ToString())
                .OrderBy(message => message.CreatedAt)
                .ToList()
            );

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
        public async Task<IActionResult> CreateChatRoom([FromBody] C_Chat request)
        {
            if (request.Members == null || !request.Members.Any())
            {
                return BadRequest("Members list cannot be empty.");
            }

            C_Chat newChatRoom = new C_Chat
            {
                Members = request.Members,
                CreatedAt = DateTime.Now
            };

            _context.C_Chats.Add(newChatRoom);
            await _context.SaveChangesAsync();

            return Ok(newChatRoom);
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
}


