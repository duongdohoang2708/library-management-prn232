using LibraryManagement.BLL.DTO.Notification;
using LibraryManagement.BLL.Services;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.API.Controllers.Notifications
{
    [ApiController]
    [Route("api/notifications")]
    public class NotificationsController : ControllerBase
    {
        private readonly NotificationService notificationService;

        public NotificationsController(NotificationService _notificationService)
        {
            notificationService = _notificationService;
        }

        [HttpGet("users/{userId:int}")]
        public async Task<IActionResult> GetUserNotifications(int userId, int page = 1)
        {
            return Ok(await notificationService.GetUserNotificationsAsync(userId, page));
        }

        [HttpPost]
        public async Task<IActionResult> Create(NotificationRequest request)
        {
            var result = await notificationService.CreateAsync(request);
            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpPost("users/{userId:int}/mark-all-read")]
        public async Task<IActionResult> MarkAllAsRead(int userId)
        {
            await notificationService.MarkAllAsReadAsync(userId);
            return Ok(new { message = "Notifications marked as read." });
        }

        [HttpPost("{id:int}/mark-read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            await notificationService.MarkAsReadAsync(id);
            return Ok(new { message = "Notification marked as read." });
        }
    }
}
