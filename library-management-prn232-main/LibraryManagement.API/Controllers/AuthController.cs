using LibraryManagement.BLL.DTO.Auth;
using LibraryManagement.BLL.Services.Interface;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            var result = await _authService.LoginAsync(request);
            if (result == null) return Unauthorized(new {message = "Fail password" });

            return Ok(result);
        }
    }
}
