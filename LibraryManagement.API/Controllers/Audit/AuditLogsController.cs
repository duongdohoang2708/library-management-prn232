using LibraryManagement.BLL.Services;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.API.Controllers.Audit
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuditLogsController : ControllerBase
    {
        private readonly AuditLogService auditLogService;

        public AuditLogsController(AuditLogService _auditLogService)
        {
            auditLogService = _auditLogService;
        }

        [HttpGet]
        public async Task<IActionResult> GetLogs(string? search, int page = 1)
        {
            return Ok(await auditLogService.GetLogsAsync(search, page));
        }
    }
}
