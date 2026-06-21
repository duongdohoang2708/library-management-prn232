using System.Net.Http.Json;
using System.Security.Claims;
using LibraryManagementDAL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.Client.Controllers
{
    public class ReviewController : Controller
    {
        private readonly IHttpClientFactory httpClientFactory;
        private readonly IConfiguration configuration;

        public ReviewController(IHttpClientFactory _httpClientFactory, IConfiguration _configuration)
        {
            httpClientFactory = _httpClientFactory;
            configuration = _configuration;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int bookId)
        {
            var client = httpClientFactory.CreateClient();
            var book = await client.GetFromJsonAsync<Book>($"{GetApiBaseUrl()}/api/books/{bookId}");
            if (book == null)
            {
                return NotFound();
            }

            var reviews = await client.GetFromJsonAsync<List<BookReview>>(
                $"{GetApiBaseUrl()}/api/reviews/books/{bookId}")
                ?? new List<BookReview>();

            BookReview? userReview = null;
            var userIdText = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userIdText, out var userId))
            {
                var userReviewResponse = await client.GetAsync(
                    $"{GetApiBaseUrl()}/api/reviews/books/{bookId}/users/{userId}");

                if (userReviewResponse.IsSuccessStatusCode &&
                    userReviewResponse.Content.Headers.ContentLength != 0)
                {
                    userReview = await userReviewResponse.Content.ReadFromJsonAsync<BookReview?>();
                }
            }

            ViewBag.Book = book;
            ViewBag.UserReview = userReview;
            return View(reviews);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Member")]
        public async Task<IActionResult> SubmitReview(int bookId, int rating, string? comment)
        {
            var userIdText = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdText, out var userId))
            {
                return RedirectToAction("Login", "Auth");
            }

            var client = httpClientFactory.CreateClient();
            var response = await client.PostAsJsonAsync($"{GetApiBaseUrl()}/api/reviews", new
            {
                userId,
                bookId,
                rating,
                comment
            });
            var result = await response.Content.ReadFromJsonAsync<ActionResponseDto>();

            TempData[response.IsSuccessStatusCode ? "SuccessMessage" : "ErrorMessage"] =
                result?.Message ?? (response.IsSuccessStatusCode ? "Review submitted." : "Submit review failed.");

            return RedirectToAction(nameof(Index), new { bookId });
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
