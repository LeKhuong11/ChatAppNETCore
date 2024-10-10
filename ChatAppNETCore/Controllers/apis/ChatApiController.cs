using ChatAppNETCore.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ChatAppNETCore.Controllers.apis
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatApiController : ControllerBase
    {
        private readonly ChatAppContext _context;

        public ChatApiController(ChatAppContext context)
        {
            _context = context;
        }

        

        [HttpPost("create")]
        public async Task<ActionResult<C_Chat>> CreateChat([FromBody] C_Chat chat)
        {
            if (chat.Members == null || chat.Members.Count == 0)
            {
                return BadRequest("Chat must have at least one member.");
            }

            _context.C_Chats.Add(chat);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetChatById), new { id = chat.Id }, chat);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<C_Chat>> GetChatById(int id)
        {
            var chat = await _context.C_Chats.FindAsync(id);

            if (chat == null)
            {
                return NotFound();
            }

            return chat;
        }
    }
}
