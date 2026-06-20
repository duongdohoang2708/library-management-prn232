using System.Net.Http.Json;
using System.Security.Claims;
using LibraryManagementDAL.DTO.Reservation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.Client.Controllers
{
    [Authorize]
    public class ReservationController : Controller
    {
        private readonly IHttpClientFactory httpClientFactory;
        private readonly IConfiguration configuration;

        public ReservationController(IHttpClientFactory _httpClientFactory, IConfiguration _configuration)
        {
            httpClientFactory = _httpClientFactory;
            configuration = _configuration;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var client = httpClientFactory.CreateClient();
            var reservations = await client.GetFromJsonAsync<List<ReservationItem>>(
                $"{GetApiBaseUrl()}/api/reservations")
                ?? new List<ReservationItem>();

            return View(reservations);
        }

        [HttpGet]
        public async Task<IActionResult> MyReservations()
        {
            var userIdText = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdText, out var userId))
            {
                return RedirectToAction("Login", "Auth");
            }

            var client = httpClientFactory.CreateClient();
            var reservations = await client.GetFromJsonAsync<List<ReservationItem>>(
                $"{GetApiBaseUrl()}/api/reservations/users/{userId}")
                ?? new List<ReservationItem>();

            return View(reservations);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int bookId)
        {
            var userIdText = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdText, out var userId))
            {
                return RedirectToAction("Login", "Auth");
            }

            var client = httpClientFactory.CreateClient();
            var response = await client.PostAsJsonAsync($"{GetApiBaseUrl()}/api/reservations", new ReservationCreateRequest
            {
                UserId = userId,
                BookId = bookId
            });
            var result = await response.Content.ReadFromJsonAsync<ReservationActionResponse>();

            TempData[response.IsSuccessStatusCode ? "SuccessMessage" : "ErrorMessage"] =
                result?.Message ?? (response.IsSuccessStatusCode ? "Reservation created." : "Create reservation failed.");

            return RedirectToAction("Details", "Books", new { id = bookId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var client = httpClientFactory.CreateClient();
            var response = await client.PostAsync($"{GetApiBaseUrl()}/api/reservations/{id}/approve", null);
            var result = await response.Content.ReadFromJsonAsync<ReservationActionResponse>();

            TempData[response.IsSuccessStatusCode ? "SuccessMessage" : "ErrorMessage"] =
                result?.Message ?? (response.IsSuccessStatusCode ? "Reservation approved." : "Approve reservation failed.");

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var client = httpClientFactory.CreateClient();
            var response = await client.PostAsync($"{GetApiBaseUrl()}/api/reservations/{id}/cancel", null);
            var result = await response.Content.ReadFromJsonAsync<ReservationActionResponse>();

            TempData[response.IsSuccessStatusCode ? "SuccessMessage" : "ErrorMessage"] =
                result?.Message ?? (response.IsSuccessStatusCode ? "Reservation cancelled." : "Cancel reservation failed.");

            return RedirectToAction(nameof(Index));
        }

        private string GetApiBaseUrl()
        {
            return configuration["ApiSettings:BaseUrl"]?.TrimEnd('/')
                ?? throw new InvalidOperationException("ApiSettings:BaseUrl is not configured.");
        }
    }
}
