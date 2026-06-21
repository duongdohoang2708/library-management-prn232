using LibraryManagement.BLL.Services;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.API.Controllers.Reminders
{
    [ApiController]
    [Route("api/[controller]")]
    public class RemindersController : ControllerBase
    {
        private readonly DueReminderService dueReminderService;

        public RemindersController(DueReminderService _dueReminderService)
        {
            dueReminderService = _dueReminderService;
        }

        [HttpPost("due")]
        public async Task<IActionResult> RunDueReminders()
        {
            var sentCount = await dueReminderService.SendDueRemindersAsync();
            return Ok(new { sentCount, message = $"{sentCount} due/overdue reminders processed." });
        }
    }
}
