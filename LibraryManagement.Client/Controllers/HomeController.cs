using System.Diagnostics;
using System.Net.Http.Json;
using LibraryManagement.Client.DTO.Books;
using Microsoft.AspNetCore.Mvc;
using LibraryManagement.Client.Models;
using LibraryManagement.Client.DTO.Auth;
using LibraryManagementDAL.Models;

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
                $"{baseUrl.TrimEnd('/')}/api/books?isActive=true&sort=newest&page=1")
                ?? new BookListApiResult();

            ViewBag.Categories = result.Categories;
            ViewBag.PopularBooks = result.Items
                .OrderByDescending(book => book.BookReviews?.Any() == true
                    ? book.BookReviews.Average(review => review.Rating)
                    : 0)
                .ThenBy(book => book.Title)
                .Take(8)
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

    public IActionResult Dashboard()
    {
        var token = HttpContext.Session.GetString("AccessToken");

        if (string.IsNullOrEmpty(token))
        {
            return RedirectToAction("Login", "Auth");
        }

        return View();
    }

    public IActionResult AdminDashboard()
    {
        var roles = HttpContext.Session.GetString("Roles");

        if (string.IsNullOrEmpty(roles))
        {
            return RedirectToAction("Login", "Auth");
        }

        if (!roles.Split(',').Contains("Admin"))
        {
            return RedirectToAction("AccessDenied", "Auth");
        }

        return View();
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

}
