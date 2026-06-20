using LibraryManagement.BLL.Services;
using LibraryManagementDAL.DTO.Circulation;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.API.Controllers.Circulation
{
    [ApiController]
    [Route("api/[controller]")]
    public class CirculationController : ControllerBase
    {
        private readonly CirculationService circulationService;
        private readonly ILogger<CirculationController> logger;

        public CirculationController(CirculationService _circulationService, ILogger<CirculationController> _logger)
        {
            circulationService = _circulationService;
            logger = _logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetTransactions(string? searchQuery, string? statusFilter, int page = 1)
        {
            try
            {
                return Ok(await circulationService.GetTransactionsAsync(searchQuery, statusFilter, page));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in GetTransactions: {Message}\n{StackTrace}", ex.Message, ex.StackTrace);
                return StatusCode(500, new { error = ex.GetType().Name, message = ex.Message, inner = ex.InnerException?.Message });
            }
        }

        [HttpGet("users")]
        public async Task<IActionResult> SearchUsers(string query)
        {
            return Ok(await circulationService.SearchUsersAsync(query));
        }

        [HttpGet("available-copies")]
        public async Task<IActionResult> GetAvailableCopies()
        {
            return Ok(await circulationService.GetAvailableCopiesAsync());
        }

        [HttpPost("borrow")]
        public async Task<IActionResult> Borrow(BorrowRequest request)
        {
            var result = await circulationService.BorrowAsync(request);
            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpGet("transactions/{transactionId:int}/return-details")]
        public async Task<IActionResult> GetReturnDetails(int transactionId)
        {
            var result = await circulationService.GetReturnDetailsAsync(transactionId);
            if (result == null)
            {
                return NotFound();
            }

            return Ok(result);
        }

        [HttpPost("transactions/{transactionId:int}/return")]
        public async Task<IActionResult> Return(int transactionId, ReturnRequest request)
        {
            var result = await circulationService.ReturnAsync(transactionId, request);
            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpPost("renew")]
        public async Task<IActionResult> Renew(RenewRequest request)
        {
            var result = await circulationService.RenewAsync(request);
            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpPost("report-issue")]
        public async Task<IActionResult> ReportIssue(ReportIssueRequest request)
        {
            var result = await circulationService.ReportIssueAsync(request);
            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
    }
}
