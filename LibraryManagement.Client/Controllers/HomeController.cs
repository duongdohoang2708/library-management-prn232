using System.Diagnostics;
using System.Net.Http.Json;
using LibraryManagement.Client.DTO.Books;
using Microsoft.AspNetCore.Mvc;
using LibraryManagement.Client.Models;
using LibraryManagement.Client.DTO.Auth;
using LibraryManagementDAL.DTO.Dashboard;
using LibraryManagementDAL.Models;
using LibraryManagement.Client.DTO.Admin;
using LibraryManagementDAL.DTO.Pagination;

namespace LibraryManagement.Client.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IHttpClientFactory httpClientFactory;
    private readonly IConfiguration configuration;

    public HomeController(
        ILogger<HomeController> logger,
        IHttpClientFactory _httpClientFactory,
        IConfiguration _configuration)
    {
        _logger = logger;
        httpClientFactory = _httpClientFactory;
        configuration = _configuration;
    }

    public async Task<IActionResult> Index()
    {
        var emptyBooks = new List<Book>();
        ViewBag.Categories = new List<Category>();
        ViewBag.PopularBooks = emptyBooks;

        var baseUrl = configuration["ApiSettings:BaseUrl"];
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            return View(emptyBooks);
        }

        try
        {
            var client = httpClientFactory.CreateClient();
            var result = await client.GetFromJsonAsync<BookListApiResult>(
                $"{baseUrl.TrimEnd('/')}/api/books?isActive=true&sort=newest&page=1&pageSize=12")
                ?? new BookListApiResult();

            ViewBag.Categories = result.Categories;
            ViewBag.PopularBooks = result.Items
                .OrderByDescending(book => book.BookReviews?.Any() == true
                    ? book.BookReviews.Average(review => review.Rating)
                    : 0)
                .ThenBy(book => book.Title)
                .Take(12)
                .ToList();

            return View(result.Items);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Could not load home page book data from API.");
            ViewBag.HomeDataError = "Could not load books right now.";
            return View(emptyBooks);
        }
    }

    public async Task<IActionResult> Dashboard()
    {
        var token = HttpContext.Session.GetString("AccessToken");

        if (string.IsNullOrEmpty(token))
        {
            return RedirectToAction("Login", "Auth");
        }

        await LoadAdminDashboardStatsAsync();
        return View();
    }

    public async Task<IActionResult> AdminDashboard()
    {
        var roles = HttpContext.Session.GetString("Roles");

        if (string.IsNullOrEmpty(roles))
        {
            return RedirectToAction("Login", "Auth");
        }

        if (!roles.Split(',').Any(role => role == "Admin" || role == "Manager"))
        {
            return RedirectToAction("AccessDenied", "Auth");
        }

        await LoadAdminDashboardStatsAsync();
        await LoadRecentAuditLogsAsync();
        await LoadLibraryPolicySettingsAsync();
        return View();
    }

    private async Task LoadRecentAuditLogsAsync()
    {
        var baseUrl = configuration["ApiSettings:BaseUrl"];
        if (!string.IsNullOrWhiteSpace(baseUrl))
        {
            try
            {
                var client = httpClientFactory.CreateClient();
                var result = await client.GetFromJsonAsync<PaginationResponseModel<AuditLogItemDto>>(
                    $"{baseUrl.TrimEnd('/')}/api/auditlogs?pageSize=5")
                    ?? new PaginationResponseModel<AuditLogItemDto>();
                ViewBag.RecentAuditLogs = result.Items ?? new List<AuditLogItemDto>();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not load recent audit logs for admin dashboard.");
                ViewBag.RecentAuditLogs = new List<AuditLogItemDto>();
            }
        }
        else
        {
            ViewBag.RecentAuditLogs = new List<AuditLogItemDto>();
        }
    }

    private async Task LoadLibraryPolicySettingsAsync()
    {
        var baseUrl = configuration["ApiSettings:BaseUrl"];
        if (!string.IsNullOrWhiteSpace(baseUrl))
        {
            try
            {
                var client = httpClientFactory.CreateClient();
                var policy = await client.GetFromJsonAsync<LibraryPolicySettingsDto>(
                    $"{baseUrl.TrimEnd('/')}/api/settings/policy")
                    ?? new LibraryPolicySettingsDto();
                ViewBag.LibraryPolicy = policy;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not load library settings for admin dashboard.");
                ViewBag.LibraryPolicy = new LibraryPolicySettingsDto();
            }
        }
        else
        {
            ViewBag.LibraryPolicy = new LibraryPolicySettingsDto();
        }
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    private async Task LoadAdminDashboardStatsAsync()
    {
        var stats = new DashboardStatsResult();
        var baseUrl = configuration["ApiSettings:BaseUrl"];

        if (!string.IsNullOrWhiteSpace(baseUrl))
        {
            try
            {
                var client = httpClientFactory.CreateClient();
                stats = await client.GetFromJsonAsync<DashboardStatsResult>(
                    $"{baseUrl.TrimEnd('/')}/api/dashboard/admin")
                    ?? new DashboardStatsResult();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not load admin dashboard statistics from API.");
                ViewBag.DashboardDataError = "Could not load dashboard statistics right now.";
            }
        }
        else
        {
            ViewBag.DashboardDataError = "ApiSettings:BaseUrl is not configured.";
        }

        ViewBag.TotalBooks = stats.TotalBooks;
        ViewBag.TotalUsers = stats.TotalUsers;
        ViewBag.TotalTransactions = stats.TotalTransactions;
        ViewBag.TotalActiveBorrows = stats.TotalActiveBorrows;
        ViewBag.OverdueBooksCount = stats.OverdueBooksCount;
        ViewBag.TotalUnpaidFines = stats.TotalUnpaidFines;
        ViewBag.MonthlyLabels = stats.MonthlyLabels;
        ViewBag.MonthlyBorrows = stats.MonthlyBorrows;
        ViewBag.MonthlyReturns = stats.MonthlyReturns;
        ViewBag.MonthlyRegistrations = stats.MonthlyRegistrations;
        ViewBag.CategoryLabels = stats.CategoryStats.Select(x => x.Label).ToList();
        ViewBag.CategoryCounts = stats.CategoryStats.Select(x => x.Count).ToList();
        ViewBag.BookStatusLabels = stats.BookStatusStats.Select(x => x.Label).ToList();
        ViewBag.BookStatusCounts = stats.BookStatusStats.Select(x => x.Count).ToList();
        ViewBag.RecentUsers = stats.RecentUsers;
        ViewBag.RecentPayments = stats.RecentPayments;
        ViewBag.RecentTransactions = stats.RecentTransactions;
        ViewBag.TopDebtors = stats.TopDebtors;
    }

}
