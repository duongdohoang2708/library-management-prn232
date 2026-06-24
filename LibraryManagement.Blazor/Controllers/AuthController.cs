using System.Net.Http.Json;
using System.Security.Claims;
using LibraryManagement.Blazor.DTO.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

namespace LibraryManagement.Blazor.Controllers;

[Route("auth")]
public class AuthController : Controller
{
    private readonly IHttpClientFactory httpClientFactory;
    private readonly IConfiguration configuration;

    public AuthController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        this.httpClientFactory = httpClientFactory;
        this.configuration = configuration;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromForm] LoginRequestDto model, string? returnUrl = null)
    {
        if (string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrWhiteSpace(model.Password))
        {
            return RedirectTo("/Auth/Login", ("error", "Email and password are required."), ("returnUrl", returnUrl));
        }

        var baseUrl = GetApiBaseUrl();
        if (baseUrl is null)
        {
            return RedirectTo("/Auth/Login", ("error", "ApiSettings:BaseUrl is not configured."), ("returnUrl", returnUrl));
        }

        var client = httpClientFactory.CreateClient();
        var response = await client.PostAsJsonAsync($"{baseUrl}/api/auth/login", new
        {
            userNameOrEmail = model.Email,
            password = model.Password
        });

        if (!response.IsSuccessStatusCode)
        {
            return RedirectTo("/Auth/Login", ("error", "Invalid email or password."), ("returnUrl", returnUrl));
        }

        var loginResult = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
        if (loginResult is null || string.IsNullOrWhiteSpace(loginResult.AccessToken))
        {
            return RedirectTo("/Auth/Login", ("error", "Login failed."), ("returnUrl", returnUrl));
        }

        HttpContext.Session.SetString("AccessToken", loginResult.AccessToken);
        HttpContext.Session.SetString("UserId", loginResult.UserId.ToString());
        HttpContext.Session.SetString("Username", loginResult.Username);
        HttpContext.Session.SetString("FullName", loginResult.FullName);
        HttpContext.Session.SetString("Email", loginResult.Email);
        HttpContext.Session.SetString("Roles", string.Join(",", loginResult.Roles));

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, loginResult.UserId.ToString()),
            new(ClaimTypes.Name, loginResult.Email),
            new(ClaimTypes.Email, loginResult.Email),
            new("Username", loginResult.Username),
            new("FullName", loginResult.FullName)
        };

        foreach (var role in loginResult.Roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var authProperties = new AuthenticationProperties
        {
            IsPersistent = model.RememberMe,
            ExpiresUtc = DateTimeOffset.UtcNow.AddHours(model.RememberMe ? 24 * 7 : 2)
        };

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity),
            authProperties);

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return LocalRedirect(returnUrl);
        }

        return Redirect(loginResult.Roles.Contains("Admin", StringComparer.OrdinalIgnoreCase)
            ? "/Home/AdminDashboard"
            : "/");
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        HttpContext.Session.Clear();
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Redirect("/Auth/Login");
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromForm] RegisterRequestDto model)
    {
        var baseUrl = GetApiBaseUrl();
        if (baseUrl is null)
        {
            return RedirectTo("/Auth/Register", ("error", "ApiSettings:BaseUrl is not configured."));
        }

        var client = httpClientFactory.CreateClient();
        var response = await client.PostAsJsonAsync($"{baseUrl}/api/auth/register", model);
        var registerResult = await response.Content.ReadFromJsonAsync<RegisterResponseDto>();

        if (!response.IsSuccessStatusCode)
        {
            return RedirectTo("/Auth/Register", ("error", registerResult?.Message ?? "Register failed."));
        }

        return RedirectTo("/Auth/Login", ("success", registerResult?.Message ?? "Registration completed."));
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromForm] ForgotPasswordRequestDto model)
    {
        var baseUrl = GetApiBaseUrl();
        if (baseUrl is null)
        {
            return RedirectTo("/Auth/ForgotPassword", ("error", "ApiSettings:BaseUrl is not configured."));
        }

        var client = httpClientFactory.CreateClient();
        var response = await client.PostAsJsonAsync($"{baseUrl}/api/auth/forgot-password", model);
        var result = await response.Content.ReadFromJsonAsync<RegisterResponseDto>();

        if (!response.IsSuccessStatusCode)
        {
            return RedirectTo("/Auth/ForgotPassword", ("error", result?.Message ?? "Could not send reset code."));
        }

        return RedirectTo("/Auth/ResetPassword",
            ("success", result?.Message ?? "Reset code sent."),
            ("email", model.Email));
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromForm] ResetPasswordRequestDto model)
    {
        var baseUrl = GetApiBaseUrl();
        if (baseUrl is null)
        {
            return RedirectTo("/Auth/ResetPassword",
                ("error", "ApiSettings:BaseUrl is not configured."),
                ("email", model.Email),
                ("token", model.Token));
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
            return RedirectTo("/Auth/ResetPassword",
                ("error", result?.Message ?? "Reset password failed."),
                ("email", model.Email),
                ("token", model.Token));
        }

        return RedirectTo("/Auth/Login", ("success", result?.Message ?? "Password reset completed."));
    }

    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromForm] ChangePasswordRequestDto model)
    {
        var userId = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Redirect("/Auth/Login");
        }

        var baseUrl = GetApiBaseUrl();
        if (baseUrl is null)
        {
            return RedirectTo("/Auth/ChangePassword", ("error", "ApiSettings:BaseUrl is not configured."));
        }

        var client = httpClientFactory.CreateClient();
        var response = await client.PostAsJsonAsync($"{baseUrl}/api/auth/change-password", new
        {
            userId = int.Parse(userId),
            currentPassword = model.CurrentPassword,
            newPassword = model.NewPassword,
            confirmNewPassword = model.ConfirmNewPassword
        });

        if (!response.IsSuccessStatusCode)
        {
            return RedirectTo("/Auth/ChangePassword", ("error", "Change password failed."));
        }

        return RedirectTo("/Auth/ChangePassword", ("success", "Password changed successfully."));
    }

    [HttpPost("login-with-google")]
    public IActionResult LoginWithGoogle()
    {
        return RedirectTo("/Auth/Login", ("error", "Google login is not configured yet."));
    }

    private string? GetApiBaseUrl()
    {
        return configuration["ApiSettings:BaseUrl"]?.TrimEnd('/');
    }

    private RedirectResult RedirectTo(string path, params (string Key, string? Value)[] queryValues)
    {
        var query = queryValues
            .Where(entry => !string.IsNullOrWhiteSpace(entry.Value))
            .ToDictionary(entry => entry.Key, entry => (string?)entry.Value);

        return Redirect(query.Count == 0 ? path : QueryHelpers.AddQueryString(path, query));
    }
}
