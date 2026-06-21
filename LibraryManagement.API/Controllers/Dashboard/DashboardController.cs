using LibraryManagement.BLL.Services;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.API.Controllers.Dashboard
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly DashboardService dashboardService;

        public DashboardController(DashboardService _dashboardService)
        {
            dashboardService = _dashboardService;
        }

        [HttpGet("admin")]
        public async Task<IActionResult> GetAdminStats()
        {
            return Ok(await dashboardService.GetStatsAsync());
        }
    }
}
