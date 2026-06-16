using System.Net.Http.Json;
using System.Security.Claims;
using LibraryManagement.Client.DTO;
using LibraryManagement.Client.DTO.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
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

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, loginResult.UserId.ToString()),
                new Claim(ClaimTypes.Name, loginResult.Email),
                new Claim(ClaimTypes.Email, loginResult.Email),
                new Claim("Username", loginResult.Username),
                new Claim("FullName", loginResult.FullName)
            };

            foreach (var role in loginResult.Roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var claimsIdentity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = model.RememberMe,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(model.RememberMe ? 24 * 7 : 2)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties
            );

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
        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Clear();
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Auth");
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View(new RegisterRequestDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterRequestDto model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var baseUrl = configuration["ApiSettings:BaseUrl"];
            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                ModelState.AddModelError(string.Empty, "Chua cau hinh ApiSettings:BaseUrl");
                return View(model);
            }

            var client = httpClientFactory.CreateClient();
            var response = await client.PostAsJsonAsync($"{baseUrl}/api/auth/register", model);

            var registerResult = await response.Content.ReadFromJsonAsync<RegisterResponseDto>();
            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(
                    string.Empty,
                    registerResult?.Message ?? "Dang ky tai khoan that bai"
                );
                return View(model);
            }

            TempData["SuccessMessage"] = registerResult?.Message ?? "Dang ky tai khoan thanh cong";
            return RedirectToAction("Login", "Auth");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordRequestDto model)
        {
            if(!ModelState.IsValid)
            {
                return View(model);
            }
            var UserId = HttpContext.Session.GetString("UserId");
            if(string.IsNullOrWhiteSpace(UserId))
            {
                return RedirectToAction("Login", "Auth");
            }

            var baseUrl = configuration["ApiSettings:BaseUrl"];
            var client = httpClientFactory.CreateClient();
            var request = new
            {
                userId = int.Parse(UserId),
                currentPassword = model.CurrentPassword,
                newPassword = model.NewPassword,
                confirmNewPassword = model.ConfirmNewPassword
            };
            var response = await client.PostAsJsonAsync($"{baseUrl}/api/auth/change-password", request);
            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(string.Empty, "Doi mat khau that bai");
                return View(model);
            }

            TempData["SuccessMessage"] = "Doi mat khau thanh cong";
            return RedirectToAction("ChangePassword", "Auth");
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View(new LibraryManagementDAL.DTO.Auth.ForgotPasswordRequestDto());
        }

        [HttpGet]
        public IActionResult ResetPassword(string? email = null, string? token = null)
        {
            return View(new LibraryManagementDAL.DTO.Auth.ResetPasswordRequestDto
            {
                Email = email ?? string.Empty,
                Token = token ?? string.Empty
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(LibraryManagementDAL.DTO.Auth.ForgotPasswordRequestDto model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var baseUrl = configuration["ApiSettings:BaseUrl"];
            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                ModelState.AddModelError(string.Empty, "Chua cau hinh ApiSettings:BaseUrl");
                return View(model);
            }

            var client = httpClientFactory.CreateClient();
            var response = await client.PostAsJsonAsync($"{baseUrl}/api/auth/forgot-password", model);
            var result = await response.Content.ReadFromJsonAsync<RegisterResponseDto>();

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(string.Empty, result?.Message ?? "Gui ma xac nhan that bai");
                return View(model);
            }

            TempData["SuccessMessage"] = result?.Message ?? "Ma xac nhan da duoc gui ve email";
            return RedirectToAction(nameof(ResetPassword), new { email = model.Email });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(LibraryManagementDAL.DTO.Auth.ResetPasswordRequestDto model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var baseUrl = configuration["ApiSettings:BaseUrl"];
            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                ModelState.AddModelError(string.Empty, "Chua cau hinh ApiSettings:BaseUrl");
                return View(model);
            }

            var client = httpClientFactory.CreateClient();
            var response = await client.PostAsJsonAsync($"{baseUrl}/api/auth/reset-password", new
            {
                email = model.Email,
                code = model.Token,
                newPassword = model.NewPassword,
                confirmNewPassword = model.ConfirmNewPassword
            });
            var result = await response.Content.ReadFromJsonAsync<RegisterResponseDto>();

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(string.Empty, result?.Message ?? "Doi mat khau that bai");
                return View(model);
            }

            TempData["SuccessMessage"] = result?.Message ?? "Doi mat khau thanh cong";
            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View(new ChangePasswordRequestDto());
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
