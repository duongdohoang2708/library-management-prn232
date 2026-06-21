using System.Net.Http.Json;
using System.Security.Claims;
using LibraryManagement.Client.Helpers;
using LibraryManagement.Client.DTO.User;
using LibraryManagementDAL.DTO.Circulation;
using LibraryManagementDAL.DTO.Pagination;
using LibraryManagementDAL.DTO.Reservation;
using LibraryManagementDAL.DTO.User;
using LibraryManagementDAL.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.Client.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        private readonly IHttpClientFactory httpClientFactory;
        private readonly IConfiguration configuration;

        public UserController(IHttpClientFactory _httpClientFactory, IConfiguration _configuration)
        {
            httpClientFactory = _httpClientFactory;
            configuration = _configuration;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> AllUsers(string? search, string? roleName, int page = 1)
        {
            var query = new List<string>
            {
                $"page={page}"
            };
            AddQuery(query, "search", search);
            AddQuery(query, "roleName", roleName);

            var client = httpClientFactory.CreateClient();
            var users = await client.GetFromJsonAsync<PaginationResponseModel<Account>>(
                $"{GetApiBaseUrl()}/api/users?{string.Join("&", query)}")
                ?? new PaginationResponseModel<Account>
                {
                    CurrentPage = 1,
                    PageNumber = 1,
                    PageSize = 10,
                    TotalPages = 1
                };

            var roles = await client.GetFromJsonAsync<List<string>>($"{GetApiBaseUrl()}/api/users/roles")
                ?? new List<string> { "Member", "Librarian", "Manager", "Admin" };

            ViewBag.Search = search;
            ViewBag.SelectedRole = roleName;
            ViewBag.Roles = roles;

            return View(users);
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Manager")]
        public IActionResult Create()
        {
            return View(new UserCreateRequest
            {
                Role = "Librarian"
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Create(UserCreateRequest model)
        {
            ModelState.Remove(nameof(UserCreateRequest.Password));
            ModelState.Remove(nameof(UserCreateRequest.Role));

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            model.Role = "Librarian";

            var client = httpClientFactory.CreateClient();
            ApiActorHeaderHelper.AddActorHeaders(client, User);
            var response = await client.PostAsJsonAsync($"{GetApiBaseUrl()}/api/users", model);
            var result = await response.Content.ReadFromJsonAsync<ApiActionResponse>();

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(string.Empty, result?.Message ?? "Create user failed.");
                return View(model);
            }

            TempData["SuccessMessage"] = result?.Message ?? "User created successfully.";
            return RedirectToAction(nameof(AllUsers));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var client = httpClientFactory.CreateClient();
            ApiActorHeaderHelper.AddActorHeaders(client, User);
            var response = await client.PostAsync($"{GetApiBaseUrl()}/api/users/{id}/toggle-status", null);
            var result = await response.Content.ReadFromJsonAsync<ApiActionResponse>();

            TempData[response.IsSuccessStatusCode ? "SuccessMessage" : "ErrorMessage"] =
                result?.Message ?? (response.IsSuccessStatusCode ? "User status updated." : "Update user status failed.");

            return RedirectToAction(nameof(AllUsers));
        }

        [HttpGet]
        public async Task<IActionResult> Profile(int? id)
        {
            var targetUserId = id;
            if (!targetUserId.HasValue)
            {
                var userIdText = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!int.TryParse(userIdText, out var currentUserId))
                {
                    return RedirectToAction("Login", "Auth");
                }

                targetUserId = currentUserId;
            }

            var client = httpClientFactory.CreateClient();
            var account = await client.GetFromJsonAsync<Account>($"{GetApiBaseUrl()}/api/users/{targetUserId.Value}");
            if (account == null)
            {
                return NotFound();
            }

            return View(new UserProfileDto
            {
                UserId = account.UserId,
                Username = account.Username,
                Email = account.Email,
                FullName = account.FullName,
                Phone = account.Phone ?? string.Empty,
                Address = account.Address ?? string.Empty,
                DateOfBirth = account.DateOfBirth,
                IsActive = account.IsActive
            });
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            return RedirectToAction("ChangePassword", "Auth");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(UserProfileDto model)
        {
            var userIdText = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdText, out var userId))
            {
                return RedirectToAction("Login", "Auth");
            }

            var client = httpClientFactory.CreateClient();
            var response = await client.PutAsJsonAsync($"{GetApiBaseUrl()}/api/users/{userId}/profile", new
            {
                model.FullName,
                model.Phone,
                model.Address,
                model.DateOfBirth
            });
            var result = await response.Content.ReadFromJsonAsync<ApiActionResponse>();

            TempData[response.IsSuccessStatusCode ? "SuccessMessage" : "ErrorMessage"] =
                result?.Message ?? (response.IsSuccessStatusCode ? "Profile updated successfully." : "Profile update failed.");

            if (response.IsSuccessStatusCode)
            {
                await RefreshCurrentUserClaimsAsync(model.FullName);
            }

            return RedirectToAction(nameof(Profile));
        }

        [HttpGet]
        [Authorize(Roles = "Member")]
        public async Task<IActionResult> MemberDashboard()
        {
            var userIdText = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdText, out var userId))
            {
                return RedirectToAction("Login", "Auth");
            }

            var client = httpClientFactory.CreateClient();
            var transactions = await client.GetFromJsonAsync<List<CirculationTransactionItem>>(
                $"{GetApiBaseUrl()}/api/circulation/users/{userId}/borrow-history")
                ?? new List<CirculationTransactionItem>();
            var reservations = await client.GetFromJsonAsync<List<ReservationItem>>(
                $"{GetApiBaseUrl()}/api/reservations/users/{userId}")
                ?? new List<ReservationItem>();
            var notifications = await client.GetFromJsonAsync<PaginationResponseModel<Notification>>(
                $"{GetApiBaseUrl()}/api/notifications/users/{userId}?page=1")
                ?? new PaginationResponseModel<Notification>();

            var openDetails = transactions
                .SelectMany(x => x.BorrowDetails)
                .Where(x => x.ActualReturnDate == null)
                .ToList();

            var model = new MemberDashboardViewModel
            {
                BorrowTransactions = transactions.Take(5).ToList(),
                Reservations = reservations.Take(5).ToList(),
                Notifications = notifications.Items.Take(5).ToList(),
                CurrentlyBorrowedCount = openDetails.Count,
                OverdueCount = openDetails.Count(x => x.DueDate.Date < DateTime.UtcNow.Date),
                UnpaidFineAmount = transactions.SelectMany(x => x.BorrowDetails).Sum(x => x.FineAmount ?? 0),
                ActiveReservationCount = reservations.Count(x => x.Status == ReservationStatus.Pending || x.Status == ReservationStatus.Allocated)
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> BorrowHistory()
        {
            var userIdText = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdText, out var userId))
            {
                return RedirectToAction("Login", "Auth");
            }

            var client = httpClientFactory.CreateClient();
            var transactions = await client.GetFromJsonAsync<List<CirculationTransactionItem>>(
                $"{GetApiBaseUrl()}/api/circulation/users/{userId}/borrow-history")
                ?? new List<CirculationTransactionItem>();

            return View(transactions);
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

        private async Task RefreshCurrentUserClaimsAsync(string fullName)
        {
            var claims = User.Claims
                .Where(x => x.Type != "FullName")
                .ToList();

            claims.Add(new Claim("FullName", fullName ?? string.Empty));

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity));

            HttpContext.Session.SetString("FullName", fullName ?? string.Empty);
        }

        private class ApiActionResponse
        {
            public bool IsSuccess { get; set; }
            public string Message { get; set; } = string.Empty;
            public int? UserId { get; set; }
        }
    }
}
