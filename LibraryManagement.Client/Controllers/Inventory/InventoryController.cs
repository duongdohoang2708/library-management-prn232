using System.Net.Http.Json;
using LibraryManagement.Client.DTO.Inventory;
using LibraryManagementDAL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.Client.Controllers.Inventory
{
    [Authorize(Roles = "Admin,Manager,Librarian")]
    public class InventoryController : Controller
    {
        private readonly IHttpClientFactory httpClientFactory;
        private readonly IConfiguration configuration;

        public InventoryController(IHttpClientFactory _httpClientFactory, IConfiguration _configuration)
        {
            httpClientFactory = _httpClientFactory;
            configuration = _configuration;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? searchBarcode, BookCopyStatus? status, BookCondition? condition, string? location, int page = 1)
        {
            var query = new List<string> { $"page={page}" };
            AddQuery(query, "searchBarcode", searchBarcode);
            AddQuery(query, "status", status.HasValue ? ((int)status.Value).ToString() : null);
            AddQuery(query, "condition", condition.HasValue ? ((int)condition.Value).ToString() : null);
            AddQuery(query, "location", location);

            var client = httpClientFactory.CreateClient();
            var result = await client.GetFromJsonAsync<InventoryListApiResult>($"{GetApiBaseUrl()}/api/inventory?{string.Join("&", query)}")
                ?? new InventoryListApiResult();

            ViewBag.SearchBarcode = searchBarcode;
            ViewBag.Status = status;
            ViewBag.Condition = condition;
            ViewBag.Location = location;
            ViewBag.CurrentPage = result.CurrentPage;
            ViewBag.TotalPages = result.TotalPages;

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_InventoryTable", result.Items);
            }

            return View(result.Items);
        }

        [HttpGet]
        public async Task<IActionResult> ManageCopies(int bookId)
        {
            var client = httpClientFactory.CreateClient();
            var result = await client.GetFromJsonAsync<BookCopiesApiResult>($"{GetApiBaseUrl()}/api/inventory/book/{bookId}");
            if (result == null)
            {
                return NotFound();
            }

            ViewBag.BookId = result.BookId;
            ViewBag.BookTitle = result.BookTitle;
            return View(result.Items);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCopy(int bookId, int numberOfCopies, string? location)
        {
            var client = httpClientFactory.CreateClient();
            var response = await client.PostAsJsonAsync($"{GetApiBaseUrl()}/api/inventory/book/{bookId}/copies", new
            {
                numberOfCopies,
                location
            });

            TempData[response.IsSuccessStatusCode ? "SuccessMessage" : "ErrorMessage"] =
                response.IsSuccessStatusCode ? $"Added {numberOfCopies} copies." : "Add copies failed.";

            return RedirectToAction(nameof(ManageCopies), new { bookId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int copyId, int bookId, BookCopyStatus status, BookCondition condition, string? location)
        {
            var client = httpClientFactory.CreateClient();
            var response = await client.PutAsJsonAsync($"{GetApiBaseUrl()}/api/inventory/copies/{copyId}", new
            {
                bookId,
                status,
                condition,
                location
            });

            TempData[response.IsSuccessStatusCode ? "SuccessMessage" : "ErrorMessage"] =
                response.IsSuccessStatusCode ? "Copy updated." : "Update copy failed.";

            return RedirectToAction(nameof(ManageCopies), new { bookId });
        }

        private string GetApiBaseUrl()
        {
            return configuration["ApiSettings:BaseUrl"]?.TrimEnd('/')
                ?? throw new InvalidOperationException("ApiSettings:BaseUrl is not configured.");
        }

        private static void AddQuery(List<string> query, string name, string? value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                query.Add($"{name}={Uri.EscapeDataString(value)}");
            }
        }

    }
}
