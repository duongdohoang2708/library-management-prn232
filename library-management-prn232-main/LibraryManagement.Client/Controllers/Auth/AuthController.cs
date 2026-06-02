using System.Net.Http.Json;
using LibraryManagement.Client.DTO;
using LibraryManagement.Client.DTO.Auth;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.Client.Controllers.Auth
{
    public class AuthController : Controller
    {
        private readonly IHttpClientFactory httpClientFactory;
        private readonly IConfiguration configuration;

        public AuthController(IHttpClientFactory _httpClientFactory, IConfiguration _configuration)
        {
            httpClientFactory = _httpClientFactory;
            configuration = _configuration;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View(new LoginRequestDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginRequestDto model, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.ReturnUrl = returnUrl;
                return View(model);
            }

            var baseUrl = configuration["ApiSettings:BaseUrl"];
            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                ModelState.AddModelError(string.Empty, "Chua cau hinh ApiSettings:BaseUrl");
                ViewBag.ReturnUrl = returnUrl;
                return View(model);
            }

            var client = httpClientFactory.CreateClient();
            var apiRequest = new
            {
                userNameOrEmail = model.Email,
                password = model.Password
            };

            var response = await client.PostAsJsonAsync($"{baseUrl}/api/auth/login", apiRequest);
            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(string.Empty, "Sai email hoac mat khau");
                ViewBag.ReturnUrl = returnUrl;
                return View(model);
            }

            var loginResult = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
            if (loginResult == null || string.IsNullOrWhiteSpace(loginResult.AccessToken))
            {
                ModelState.AddModelError(string.Empty, "Dang nhap that bai");
                ViewBag.ReturnUrl = returnUrl;
                return View(model);
            }

            HttpContext.Session.SetString("AccessToken", loginResult.AccessToken);
            HttpContext.Session.SetString("UserId", loginResult.UserId.ToString());
            HttpContext.Session.SetString("Username", loginResult.Username);
            HttpContext.Session.SetString("FullName", loginResult.FullName);
            HttpContext.Session.SetString("Email", loginResult.Email);
            HttpContext.Session.SetString("Roles", string.Join(",", loginResult.Roles));

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            if (loginResult.Roles.Contains("Admin"))
            {
                return RedirectToAction("AdminDashboard", "Home");
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Auth");
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpGet]
        public IActionResult ResetPassword()
        {
            return View();
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
        [HttpPost]
        public IActionResult LoginWithGoogle()
        {
            return BadRequest(new { message = "Google login chua duoc cau hinh" });
        }
    }
}
