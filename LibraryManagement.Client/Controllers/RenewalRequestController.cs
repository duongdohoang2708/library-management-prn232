using System.Net.Http.Json;
using System.Security.Claims;
using LibraryManagement.Client.Helpers;
using LibraryManagementDAL.DTO.RenewalRequest;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.Client.Controllers
{
    [Authorize]
    public class RenewalRequestController : Controller
    {
        private readonly IHttpClientFactory httpClientFactory;
        private readonly IConfiguration configuration;

        public RenewalRequestController(IHttpClientFactory _httpClientFactory, IConfiguration _configuration)
        {
            httpClientFactory = _httpClientFactory;
            configuration = _configuration;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Manager,Librarian")]
        public IActionResult Index()
        {
            return RedirectToAction("Index", "Circulation");
        }

        [HttpGet]
        [Authorize(Roles = "Member")]
        public IActionResult MyRequests()
        {
            return RedirectToAction("BorrowHistory", "User");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager,Librarian")]
        public async Task<IActionResult> Approve(int id)
        {
            var userIdText = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdText, out var reviewerUserId))
            {
                return RedirectToAction("Login", "Auth");
            }

            var client = httpClientFactory.CreateClient();
            ApiActorHeaderHelper.AddActorHeaders(client, User);
            var response = await client.PostAsJsonAsync(
                $"{GetApiBaseUrl()}/api/renewal-requests/{id}/approve",
                new RenewalRequestApproveRequest { ReviewerUserId = reviewerUserId });
            var result = await response.Content.ReadFromJsonAsync<RenewalRequestActionResponse>();

            TempData[response.IsSuccessStatusCode ? "SuccessMessage" : "ErrorMessage"] =
                result?.Message ?? (response.IsSuccessStatusCode ? "Renewal request approved." : "Approve renewal request failed.");

            return RedirectToAction("Index", "Circulation");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager,Librarian")]
        public async Task<IActionResult> Reject(int id, string reason)
        {
            var userIdText = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdText, out var reviewerUserId))
            {
                return RedirectToAction("Login", "Auth");
            }

            var client = httpClientFactory.CreateClient();
            ApiActorHeaderHelper.AddActorHeaders(client, User);
            var response = await client.PostAsJsonAsync(
                $"{GetApiBaseUrl()}/api/renewal-requests/{id}/reject",
                new RenewalRequestRejectRequest { ReviewerUserId = reviewerUserId, Reason = reason });
            var result = await response.Content.ReadFromJsonAsync<RenewalRequestActionResponse>();

            TempData[response.IsSuccessStatusCode ? "SuccessMessage" : "ErrorMessage"] =
                result?.Message ?? (response.IsSuccessStatusCode ? "Renewal request rejected." : "Reject renewal request failed.");

            return RedirectToAction("Index", "Circulation");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Member")]
        public async Task<IActionResult> Cancel(int id)
        {
            var userIdText = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdText, out var userId))
            {
                return RedirectToAction("Login", "Auth");
            }

            var client = httpClientFactory.CreateClient();
            ApiActorHeaderHelper.AddActorHeaders(client, User);
            var response = await client.PostAsync(
                $"{GetApiBaseUrl()}/api/renewal-requests/{id}/cancel?userId={userId}",
                null);
            var result = await response.Content.ReadFromJsonAsync<RenewalRequestActionResponse>();

            TempData[response.IsSuccessStatusCode ? "SuccessMessage" : "ErrorMessage"] =
                result?.Message ?? (response.IsSuccessStatusCode ? "Renewal request cancelled." : "Cancel renewal request failed.");

            return RedirectToAction("BorrowHistory", "User");
        }

        private string GetApiBaseUrl()
        {
            return configuration["ApiSettings:BaseUrl"]?.TrimEnd('/')
                ?? throw new InvalidOperationException("ApiSettings:BaseUrl is not configured.");
        }
    }
}
