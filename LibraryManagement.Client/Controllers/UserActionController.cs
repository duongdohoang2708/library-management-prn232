using System.Net.Http.Json;
using System.Security.Claims;
using LibraryManagement.Client.Helpers;
using LibraryManagementDAL.DTO.Circulation;
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
        [Authorize(Roles = "Member")]
        public async Task<IActionResult> RequestReserve(int bookId)
        {
            var userIdText = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdText, out var userId))
            {
                return RedirectToAction("Login", "Auth");
            }

            var client = httpClientFactory.CreateClient();
            ApiActorHeaderHelper.AddActorHeaders(client, User);
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
                    ? "Your reservation has been placed."
                    : "Failed to create reservation.");
            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessLinkController"] = "Reservation";
                TempData["SuccessLinkAction"] = "MyReservations";
                TempData["SuccessLinkText"] = "View My Reservations";
            }

            return RedirectToAction("Details", "Books", new { id = bookId });
        }

        /// <summary>
        /// Member views their borrow cart page.
        /// GET /UserAction/BorrowCart
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Member")]
        public IActionResult BorrowCart()
        {
            return View();
        }

        /// <summary>
        /// Member submits cart to self-checkout multiple books in one transaction.
        /// POST /UserAction/CheckoutCart
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Member")]
        public async Task<IActionResult> CheckoutCart([FromForm] List<int> bookIds)
        {
            var userIdText = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdText, out var userId))
            {
                return RedirectToAction("Login", "Auth");
            }

            if (bookIds == null || !bookIds.Any())
            {
                TempData["ErrorMessage"] = "Your reading list is empty. Please add books before reserving.";
                return RedirectToAction(nameof(BorrowCart));
            }

            var client = httpClientFactory.CreateClient();
            ApiActorHeaderHelper.AddActorHeaders(client, User);
            var response = await client.PostAsJsonAsync(
                $"{GetApiBaseUrl()}/api/circulation/member-borrow",
                new MemberBorrowRequest
                {
                    UserId = userId,
                    BookIds = bookIds
                });

            var result = await response.Content.ReadFromJsonAsync<CirculationActionResponse>();

            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = result?.Message ?? "Reservation failed. Please try again.";
                return RedirectToAction(nameof(BorrowCart));
            }

            TempData["SuccessMessage"] = result?.Message ?? "Books reserved successfully!";
            return RedirectToAction("BorrowHistory", "User");
        }



        private string GetApiBaseUrl()
        {
            return configuration["ApiSettings:BaseUrl"]?.TrimEnd('/')
                ?? throw new InvalidOperationException("ApiSettings:BaseUrl is not configured.");
        }
    }
}
