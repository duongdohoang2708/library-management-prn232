using LibraryManagement.Blazor.DTO.Books;
using LibraryManagement.Blazor.Models;
using LibraryManagementDAL.DTO.Dashboard;
using LibraryManagementDAL.Models;

namespace LibraryManagement.Blazor.Services;

public sealed class HomeClientService : ApiClientBase
{
    private readonly ILogger<HomeClientService> logger;

    public HomeClientService(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        IHttpContextAccessor httpContextAccessor,
        ILogger<HomeClientService> logger)
        : base(httpClientFactory, configuration, httpContextAccessor)
    {
        this.logger = logger;
    }

    public async Task<HomePageViewModel> GetHomeAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await GetAsync<BookListApiResult>(
                "api/books?isActive=true&sort=newest&page=1&pageSize=12",
                cancellationToken)
                ?? new BookListApiResult();

            var popularBooks = result.Items
                .OrderByDescending(book => book.BookReviews?.Any() == true
                    ? book.BookReviews.Average(review => review.Rating)
                    : 0)
                .ThenBy(book => book.Title)
                .Take(12)
                .ToList();

            return new HomePageViewModel
            {
                Books = result.Items,
                Categories = result.Categories,
                PopularBooks = popularBooks
            };
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Could not load home page book data from API.");
            return new HomePageViewModel
            {
                ErrorMessage = "Could not load books right now."
            };
        }
    }

    public async Task<AdminDashboardViewModel> GetAdminDashboardAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var stats = await GetAsync<DashboardStatsResult>("api/dashboard/admin", cancellationToken)
                ?? new DashboardStatsResult();

            return new AdminDashboardViewModel
            {
                Stats = stats
            };
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Could not load admin dashboard statistics from API.");
            return new AdminDashboardViewModel
            {
                ErrorMessage = "Could not load dashboard statistics right now."
            };
        }
    }

    public async Task<DashboardPageViewModel> GetDashboardAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var stats = await GetAsync<DashboardStatsResult>("api/dashboard/admin", cancellationToken)
                ?? new DashboardStatsResult();

            return new DashboardPageViewModel
            {
                Stats = stats
            };
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Could not load staff dashboard statistics from API.");
            return new DashboardPageViewModel
            {
                ErrorMessage = "Could not load dashboard statistics right now."
            };
        }
    }
}
