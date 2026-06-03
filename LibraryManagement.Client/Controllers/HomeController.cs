using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using LibraryManagement.Client.Models;
using LibraryManagement.Client.DTO.Auth;

namespace LibraryManagement.Client.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
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
