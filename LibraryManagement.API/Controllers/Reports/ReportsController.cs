using LibraryManagement.BLL.Services;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.API.Controllers.Reports
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportsController : ControllerBase
    {
        private readonly ReportService reportService;

        public ReportsController(ReportService _reportService)
        {
            reportService = _reportService;
        }

        [HttpGet("advanced")]
        public async Task<IActionResult> GetAdvanced(DateTime? from, DateTime? to)
        {
            return Ok(await reportService.GetAdvancedReportAsync(from, to));
        }

        [HttpGet("advanced/export")]
        public async Task<IActionResult> ExportAdvanced(string format = "excel", DateTime? from = null, DateTime? to = null)
        {
            var result = await reportService.ExportAsync(format, from, to);
            return File(result.Bytes, result.ContentType, result.FileName);
        }
    }
}
