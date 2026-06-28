using System.Net.Http.Json;
using System.Security.Claims;
using LibraryManagement.Client.Helpers;
using LibraryManagementDAL.DTO.Circulation;
using LibraryManagementDAL.DTO.RenewalRequest;
using LibraryManagementDAL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.Client.Controllers
{
    [Authorize(Roles = "Admin,Manager,Librarian")]
    public class CirculationController : Controller
    {
        private readonly IHttpClientFactory httpClientFactory;
        private readonly IConfiguration configuration;

        public CirculationController(IHttpClientFactory _httpClientFactory, IConfiguration _configuration)
        {
            httpClientFactory = _httpClientFactory;
            configuration = _configuration;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? searchQuery, string? statusFilter, int page = 1)
        {
            var query = new List<string> { $"page={page}" };
            AddQuery(query, "searchQuery", searchQuery);
            AddQuery(query, "statusFilter", statusFilter);

            var client = httpClientFactory.CreateClient();
            var result = await client.GetFromJsonAsync<CirculationListResult>(
                $"{GetApiBaseUrl()}/api/circulation?{string.Join("&", query)}")
                ?? new CirculationListResult();

            ViewBag.SearchQuery = searchQuery;
            ViewBag.StatusFilter = statusFilter;
            ViewBag.CurrentPage = result.CurrentPage;
            ViewBag.TotalPages = result.TotalPages;

            int defaultLoanDays = 14;
            try
            {
                var policyNode = await client.GetFromJsonAsync<System.Text.Json.Nodes.JsonNode>($"{GetApiBaseUrl()}/api/settings/policy");
                if (policyNode?["defaultLoanDays"] != null)
                {
                    defaultLoanDays = policyNode["defaultLoanDays"].GetValue<int>();
                }
            }
            catch { }

            ViewBag.DefaultLoanDays = defaultLoanDays;

            var renewalRequests = await client.GetFromJsonAsync<List<RenewalRequestItem>>(
                $"{GetApiBaseUrl()}/api/renewal-requests")
                ?? new List<RenewalRequestItem>();
            ViewBag.PendingRenewalRequests = renewalRequests;

            return View(result.Items);
        }

        [HttpGet]
        public async Task<IActionResult> Borrow()
        {
            await LoadAvailableCopiesAsync();
            return View(new BorrowRequest());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Borrow(BorrowRequest model)
        {
            if (!model.Barcodes.Any())
            {
                ModelState.AddModelError(string.Empty, "Please select at least one book.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAvailableCopiesAsync();
                return View(model);
            }

            var client = httpClientFactory.CreateClient();
            ApiActorHeaderHelper.AddActorHeaders(client, User);
            var response = await client.PostAsJsonAsync($"{GetApiBaseUrl()}/api/circulation/borrow", model);
            var result = await response.Content.ReadFromJsonAsync<CirculationActionResponse>();

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(string.Empty, result?.Message ?? "Create loan failed.");
                await LoadAvailableCopiesAsync();
                return View(model);
            }

            TempData["SuccessMessage"] = result?.Message ?? "Borrow transaction created successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Return(int id)
        {
            var client = httpClientFactory.CreateClient();
            var result = await client.GetFromJsonAsync<ReturnDetailsResult>(
                $"{GetApiBaseUrl()}/api/circulation/transactions/{id}/return-details");

            if (result == null)
            {
                return NotFound();
            }

            PrepareReturnViewBag(result);

            int defaultLoanDays = 14;
            try
            {
                var policyNode = await client.GetFromJsonAsync<System.Text.Json.Nodes.JsonNode>($"{GetApiBaseUrl()}/api/settings/policy");
                if (policyNode?["defaultLoanDays"] != null)
                {
                    defaultLoanDays = policyNode["defaultLoanDays"].GetValue<int>();
                }
            }
            catch { }
            ViewBag.DefaultLoanDays = defaultLoanDays;

            return View(new ReturnRequest
            {
                BorrowDetailIds = result.UnreturnedDetails.Select(x => x.BorrowDetailId).ToList()
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Return(int id, ReturnRequest model)
        {
            if (!model.BorrowDetailIds.Any())
            {
                ModelState.AddModelError(string.Empty, "Please select at least one book to return.");
            }

            if (!ModelState.IsValid)
            {
                await ReloadReturnViewAsync(id);
                return View(model);
            }

            var client = httpClientFactory.CreateClient();
            ApiActorHeaderHelper.AddActorHeaders(client, User);
            var response = await client.PostAsJsonAsync($"{GetApiBaseUrl()}/api/circulation/transactions/{id}/return", model);
            var result = await response.Content.ReadFromJsonAsync<CirculationActionResponse>();

            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = result?.Message ?? "Return books failed.";
                await ReloadReturnViewAsync(id);
                return View(model);
            }

            TempData["SuccessMessage"] = result?.Message ?? "Books returned successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Renew(int borrowDetailId, int transactionId, int extraDays = 7, string? redirectToAction = null)
        {
            var client = httpClientFactory.CreateClient();
            ApiActorHeaderHelper.AddActorHeaders(client, User);
            var response = await client.PostAsJsonAsync($"{GetApiBaseUrl()}/api/circulation/renew", new RenewRequest
            {
                BorrowDetailId = borrowDetailId,
                ExtraDays = extraDays
            });
            var result = await response.Content.ReadFromJsonAsync<CirculationActionResponse>();

            TempData[response.IsSuccessStatusCode ? "SuccessMessage" : "ErrorMessage"] =
                result?.Message ?? (response.IsSuccessStatusCode ? "Book renewed successfully." : "Renew book failed.");

            if (redirectToAction == "Return")
            {
                return RedirectToAction("Return", new { id = transactionId });
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveRenewalRequest(int id)
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

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectRenewalRequest(int id, string reason)
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

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReportIssue(int transactionId, int borrowDetailId, BookCopyStatus status, decimal fineAmount)
        {
            var client = httpClientFactory.CreateClient();
            ApiActorHeaderHelper.AddActorHeaders(client, User);
            var response = await client.PostAsJsonAsync($"{GetApiBaseUrl()}/api/circulation/report-issue", new ReportIssueRequest
            {
                BorrowDetailId = borrowDetailId,
                Status = status,
                FineAmount = fineAmount
            });
            var result = await response.Content.ReadFromJsonAsync<CirculationActionResponse>();

            TempData[response.IsSuccessStatusCode ? "SuccessMessage" : "ErrorMessage"] =
                result?.Message ?? (response.IsSuccessStatusCode ? "Issue reported successfully." : "Report issue failed.");

            return RedirectToAction(nameof(Return), new { id = transactionId });
        }

        [HttpGet]
        public async Task<IActionResult> SearchUsers(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return Json(Array.Empty<UserSearchResult>());
            }

            var client = httpClientFactory.CreateClient();
            var users = await client.GetFromJsonAsync<List<UserSearchResult>>(
                $"{GetApiBaseUrl()}/api/circulation/users?query={Uri.EscapeDataString(query)}")
                ?? new List<UserSearchResult>();

            return Json(users);
        }

        private async Task LoadAvailableCopiesAsync()
        {
            var client = httpClientFactory.CreateClient();
            ViewBag.AvailableCopies = await client.GetFromJsonAsync<List<AvailableCopyResult>>(
                $"{GetApiBaseUrl()}/api/circulation/available-copies")
                ?? new List<AvailableCopyResult>();
        }

        private async Task ReloadReturnViewAsync(int transactionId)
        {
            var client = httpClientFactory.CreateClient();
            var result = await client.GetFromJsonAsync<ReturnDetailsResult>(
                $"{GetApiBaseUrl()}/api/circulation/transactions/{transactionId}/return-details");

            if (result != null)
            {
                PrepareReturnViewBag(result);

                int defaultLoanDays = 14;
                try
                {
                    var policyNode = await client.GetFromJsonAsync<System.Text.Json.Nodes.JsonNode>($"{GetApiBaseUrl()}/api/settings/policy");
                    if (policyNode?["defaultLoanDays"] != null)
                    {
                        defaultLoanDays = policyNode["defaultLoanDays"].GetValue<int>();
                    }
                }
                catch { }
                ViewBag.DefaultLoanDays = defaultLoanDays;
            }
        }

        private void PrepareReturnViewBag(ReturnDetailsResult result)
        {
            ViewBag.TransactionId = result.TransactionId;
            ViewBag.UserId = result.UserId;
            ViewBag.UserFullName = result.UserFullName;
            ViewBag.UnreturnedDetails = result.UnreturnedDetails;
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
