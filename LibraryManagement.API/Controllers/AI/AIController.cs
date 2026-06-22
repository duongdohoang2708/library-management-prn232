using LibraryManagement.BLL.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace LibraryManagement.API.Controllers.AI
{
    [Route("api/[controller]")]
    [ApiController]
    public class AIController : ControllerBase
    {
        private readonly IAIService _aiService;

        public AIController(IAIService aiService)
        {
            _aiService = aiService;
        }

        [HttpGet("books/{bookId}/summary")]
        public async Task<IActionResult> GetBookSummary(int bookId)
        {
            var userIdString = Request.Headers["X-Actor-UserId"].FirstOrDefault() 
                               ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int? currentUserId = int.TryParse(userIdString, out var uid) ? uid : null;

            var summary = await _aiService.GetBookSummaryAsync(bookId);
            if (summary == null)
            {
                // Auto-generate on the fly if not exists
                summary = await _aiService.GenerateBookSummaryAsync(bookId, currentUserId);
            }
            
            if (summary == null) return NotFound(new { message = "Không tìm thấy sách." });
            
            return Ok(new { summaryText = summary.SummaryText });
        }

        public class ChatRequest
        {
            public string Message { get; set; }
        }

        [HttpPost("chat")]
        public async Task<IActionResult> Chat([FromBody] ChatRequest request)
        {
            var userIdString = Request.Headers["X-Actor-UserId"].FirstOrDefault() 
                               ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int? currentUserId = int.TryParse(userIdString, out var uid) ? uid : null;

            if (string.IsNullOrWhiteSpace(request.Message))
            {
                return BadRequest(new { message = "Tin nhắn không được để trống." });
            }

            var response = await _aiService.ChatWithLibrarianAIAsync(request.Message, currentUserId);
            return Ok(new { responseText = response });
        }
    }
}
