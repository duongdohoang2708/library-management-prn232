using System.Net.Http.Json;
using System.Security.Claims;
using LibraryManagementDAL.DTO.Reservation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.Client.Controllers
{
    [Authorize]
    public class UserActionController : Controller
    {
        private readonly IHttpClientFactory httpClientFactory;
        private readonly IConfiguration configuration;

        public UserActionController(IHttpClientFactory _httpClientFactory, IConfiguration _configuration)
        {
            httpClientFactory = _httpClientFactory;
            configuration = _configuration;
        }

        /// <summary>
        /// Member đặt chỗ (Reserve) khi sách hết bản copy.
        /// POST /UserAction/RequestReserve
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestReserve(int bookId)
        {
            var userIdText = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdText, out var userId))
            {
                return RedirectToAction("Login", "Auth");
            }

            var client = httpClientFactory.CreateClient();
            var response = await client.PostAsJsonAsync(
                $"{GetApiBaseUrl()}/api/reservations",
                new ReservationCreateRequest
                {
                    UserId = userId,
                    BookId = bookId
                });

            var result = await response.Content.ReadFromJsonAsync<ReservationActionResponse>();

            TempData[response.IsSuccessStatusCode ? "SuccessMessage" : "ErrorMessage"] =
                result?.Message ?? (response.IsSuccessStatusCode
                    ? "Reservation created successfully."
                    : "Failed to create reservation.");

            return RedirectToAction("Details", "Books", new { id = bookId });
        }

        /// <summary>
        /// Member mượn sách trực tiếp khi còn bản copy.
        /// POST /UserAction/RequestBorrow
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestBorrow(int bookId)
        {
            var userIdText = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdText, out var userId))
            {
                return RedirectToAction("Login", "Auth");
            }

            var client = httpClientFactory.CreateClient();
            var response = await client.PostAsJsonAsync(
                $"{GetApiBaseUrl()}/api/reservations",
                new ReservationCreateRequest
                {
                    UserId = userId,
                    BookId = bookId
                });

            var result = await response.Content.ReadFromJsonAsync<ReservationActionResponse>();

            TempData[response.IsSuccessStatusCode ? "SuccessMessage" : "ErrorMessage"] =
                result?.Message ?? (response.IsSuccessStatusCode
                    ? "Borrow request submitted successfully."
                    : "Failed to submit borrow request.");

            return RedirectToAction("Details", "Books", new { id = bookId });
        }

        private string GetApiBaseUrl()
        {
            return configuration["ApiSettings:BaseUrl"]?.TrimEnd('/')
                ?? throw new InvalidOperationException("ApiSettings:BaseUrl is not configured.");
        }
    }
}
