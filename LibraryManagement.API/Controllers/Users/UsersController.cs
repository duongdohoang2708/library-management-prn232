using LibraryManagement.BLL.DTO.User;
using LibraryManagement.BLL.Services;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.API.Controllers.Users
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly UserManagementService userManagementService;

        public UsersController(UserManagementService _userManagementService)
        {
            userManagementService = _userManagementService;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers(string? search, string? roleName, int page = 1)
        {
            return Ok(await userManagementService.GetUsersAsync(search, roleName, page));
        }

        [HttpGet("roles")]
        public async Task<IActionResult> GetRoles()
        {
            return Ok(await userManagementService.GetRoleNamesAsync());
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetProfile(int id)
        {
            var user = await userManagementService.GetProfileAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            return Ok(user);
        }

        [HttpPost]
        public async Task<IActionResult> Create(UserCreateRequest request)
        {
            var result = await userManagementService.CreateAsync(request);
            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpPost("{id:int}/toggle-status")]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var result = await userManagementService.ToggleStatusAsync(id);
            if (!result.IsSuccess)
            {
                return NotFound(result);
            }

            return Ok(result);
        }

        [HttpPut("{id:int}/profile")]
        public async Task<IActionResult> UpdateProfile(int id, UserProfileUpdateRequest request)
        {
            var result = await userManagementService.UpdateProfileAsync(id, request);
            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
    }
}
