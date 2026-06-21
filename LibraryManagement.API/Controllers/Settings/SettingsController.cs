using LibraryManagement.BLL.DTO.Settings;
using LibraryManagement.BLL.Services;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.API.Controllers.Settings
{
    [ApiController]
    [Route("api/[controller]")]
    public class SettingsController : ControllerBase
    {
        private readonly SystemSettingService systemSettingService;

        public SettingsController(SystemSettingService _systemSettingService)
        {
            systemSettingService = _systemSettingService;
        }

        [HttpGet("policy")]
        public async Task<IActionResult> GetPolicy()
        {
            return Ok(await systemSettingService.GetPolicyAsync());
        }

        [HttpPut("policy")]
        public async Task<IActionResult> UpdatePolicy(LibraryPolicySettings request)
        {
            var result = await systemSettingService.UpdatePolicyAsync(request);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
    }
}
