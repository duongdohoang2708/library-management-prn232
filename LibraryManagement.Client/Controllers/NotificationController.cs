using System.Net.Http.Json;
using System.Security.Claims;
using LibraryManagementDAL.DTO.Notification;
using LibraryManagementDAL.DTO.Pagination;
using LibraryManagementDAL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.Client.Controllers
{
    [Authorize]
    public class NotificationController : Controller
    {
        private readonly IHttpClientFactory httpClientFactory;
        private readonly IConfiguration configuration;

        public NotificationController(IHttpClientFactory _httpClientFactory, IConfiguration _configuration)
        {
            httpClientFactory = _httpClientFactory;
            configuration = _configuration;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int page = 1)
        {
            var userIdText = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdText, out var userId))
            {
                return RedirectToAction("Login", "Auth");
            }

            var client = httpClientFactory.CreateClient();
            var result = await client.GetFromJsonAsync<PaginationResponseModel<Notification>>(
                $"{GetApiBaseUrl()}/api/notifications/users/{userId}?page={page}")
                ?? new PaginationResponseModel<Notification>
                {
                    CurrentPage = 1,
                    TotalPages = 1,
                    PageSize = 10
                };

            return View(result);
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Manager,Librarian")]
        public async Task<IActionResult> SendNotification(int? userId)
        {
            var model = new NotificationCreateRequest
            {
                Type = "General"
            };

            if (userId.HasValue && userId.Value > 0)
            {
                try
                {
                    var client = httpClientFactory.CreateClient();
                    var account = await client.GetFromJsonAsync<Account>($"{GetApiBaseUrl()}/api/users/{userId.Value}");
                    if (account != null)
                    {
                        model.UserId = userId.Value;
                        ViewBag.SelectedUserName = account.FullName;
                        ViewBag.SelectedUserEmail = account.Email;
                    }
                }
                catch { }
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager,Librarian")]
        public async Task<IActionResult> SendNotification(NotificationCreateRequest model)
        {
            if (model.UserId == null || model.UserId <= 0)
            {
                ModelState.AddModelError(nameof(model.UserId), "Please select a member.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var client = httpClientFactory.CreateClient();
            var response = await client.PostAsJsonAsync($"{GetApiBaseUrl()}/api/notifications", model);
            var result = await response.Content.ReadFromJsonAsync<ActionResponseDto>();

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(string.Empty, result?.Message ?? "Send notification failed.");
                return View(model);
            }

            TempData["Success"] = result?.Message ?? "Notification sent.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userIdText = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdText, out var userId))
            {
                return RedirectToAction("Login", "Auth");
            }

            var client = httpClientFactory.CreateClient();
            await client.PostAsync($"{GetApiBaseUrl()}/api/notifications/users/{userId}/mark-all-read", null);

            TempData["Success"] = "All notifications marked as read.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> GetLatestNotifications()
        {
            var userIdText = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdText, out var userId))
            {
                return Unauthorized();
            }

            var client = httpClientFactory.CreateClient();
            var result = await client.GetFromJsonAsync<PaginationResponseModel<Notification>>(
                $"{GetApiBaseUrl()}/api/notifications/users/{userId}?page=1")
                ?? new PaginationResponseModel<Notification>();

            var unreadCount = result.Items?.Count(x => !x.IsRead) ?? 0;
            return Json(new {
                items = result.Items ?? new List<Notification>(),
                unreadCount = unreadCount
            });
        }

        [HttpPost]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var client = httpClientFactory.CreateClient();
            var response = await client.PostAsync($"{GetApiBaseUrl()}/api/notifications/{id}/mark-read", null);
            if (response.IsSuccessStatusCode)
            {
                return Ok();
            }
            return BadRequest();
        }

        [HttpPost]
        public async Task<IActionResult> MarkAllAsReadJson()
        {
            var userIdText = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdText, out var userId))
            {
                return Unauthorized();
            }

            var client = httpClientFactory.CreateClient();
            var response = await client.PostAsync($"{GetApiBaseUrl()}/api/notifications/users/{userId}/mark-all-read", null);
            if (response.IsSuccessStatusCode)
            {
                return Ok();
            }
            return BadRequest();
        }

        private string GetApiBaseUrl()
        {
            return configuration["ApiSettings:BaseUrl"]?.TrimEnd('/')
                ?? throw new InvalidOperationException("ApiSettings:BaseUrl is not configured.");
        }

        private class ActionResponseDto
        {
            public bool IsSuccess { get; set; }
            public string Message { get; set; } = string.Empty;
            public int? Id { get; set; }
        }
    }
}
