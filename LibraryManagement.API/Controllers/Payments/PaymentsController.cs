using LibraryManagementDAL.DTO.Payment;
using LibraryManagement.BLL.Services;
using LibraryManagement.BLL.Services.Interface;
using LibraryManagementDAL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace LibraryManagement.API.Controllers.Payments
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentsController : ControllerBase
    {
        private readonly PaymentService _paymentService;
        private readonly IVnPayService _vnPayService;
        private readonly IConfiguration _configuration;

        public PaymentsController(PaymentService paymentService, IVnPayService vnPayService, IConfiguration configuration)
        {
            _paymentService = paymentService;
            _vnPayService = vnPayService;
            _configuration = configuration;
        }

        [HttpGet("users-with-fines")]
        public async Task<IActionResult> GetUsersWithFines([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null)
        {
            var result = await _paymentService.GetPagedUsersWithUnpaidFinesAsync(page, pageSize, search);
            return Ok(new PagedUserFinesResponse
            {
                Users = result.Users,
                TotalCount = result.TotalCount,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling((double)result.TotalCount / pageSize)
            });
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUnpaidFinesByUserId(int userId)
        {
            var fines = await _paymentService.GetUnpaidFinesByUserIdAsync(userId);
            return Ok(fines);
        }

        [HttpGet("my-fines")]
        public async Task<IActionResult> GetMyFines()
        {
            var userIdStr = Request.Headers["X-Actor-UserId"].FirstOrDefault();
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
                return Unauthorized();

            var fines = await _paymentService.GetMyFinesAsync(userId);
            return Ok(fines);
        }
        
        [HttpGet("my-history")]
        public async Task<IActionResult> GetMyPaymentHistory()
        {
            var userIdStr = Request.Headers["X-Actor-UserId"].FirstOrDefault();
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
                return Unauthorized();

            var history = await _paymentService.GetMyPaymentHistoryAsync(userId);
            return Ok(history);
        }

        [HttpPost("process-cash")]
        public async Task<IActionResult> ProcessCashPayment([FromBody] ProcessPaymentRequestDTO request)
        {
            try
            {
                var payment = await _paymentService.ProcessPaymentAsync(request.UserId, request.BorrowDetailIds, request.AmountPaid, PaymentMethod.Cash);
                return Ok(new { success = true, paymentId = payment.PaymentId, transactionCode = payment.TransactionCode });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost("manual-fine")]
        public async Task<IActionResult> CreateManualFine([FromBody] CreateManualFineRequestDTO request)
        {
            try
            {
                var detail = await _paymentService.CreateManualFineAsync(request.UserId, request.Amount, request.Reason);
                return Ok(new { success = true, borrowDetailId = detail.BorrowDetailId });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost("vnpay-url")]
        public async Task<IActionResult> CreateVnPayUrl([FromBody] CreateVnPayUrlRequestDTO request)
        {
            try
            {
                var actorIdStr = Request.Headers["X-Actor-UserId"].FirstOrDefault();
                if (string.IsNullOrEmpty(actorIdStr) || !int.TryParse(actorIdStr, out int actorId))
                    return Unauthorized();

                int targetUserId = request.UserId > 0 ? request.UserId : actorId;

                if (request.Amount <= 0) return BadRequest("Amount must be greater than 0");

                var transactionRef = $"VNP-{targetUserId}-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString().Substring(0,4)}";
                
                // Save borrowDetailIds somewhere?
                // We should pass them via OrderInfo or state so we know what to pay when VnPay returns.
                // VnPay OrderInfo has max length 255.
                string detailIdsStr = string.Join(",", request.BorrowDetailIds);
                string orderInfo = $"UID:{targetUserId}|BD:{detailIdsStr}";
                if (orderInfo.Length > 250) {
                    // Truncate or use a temporary table to store the intent
                    orderInfo = orderInfo.Substring(0, 250);
                }

                var url = _vnPayService.CreatePaymentUrl(HttpContext, transactionRef, request.Amount, orderInfo);
                
                return Ok(new { success = true, url });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("vnpay-return")]
        public async Task<IActionResult> VnPayReturn()
        {
            try
            {
                var response = _vnPayService.PaymentExecute(Request.Query);
                
                // Get Frontend URL from config
                var frontendUrl = _configuration["ClientUrl"] ?? "https://localhost:7119"; 

                // Parse orderInfo to get UserId and BorrowDetailIds
                // Format: UID:1|BD:1,2,3
                int userId = 0;
                List<int> detailIds = new List<int>();
                if (!string.IsNullOrEmpty(response.OrderDescription))
                {
                    var parts = response.OrderDescription.Split('|');
                    foreach(var part in parts) {
                        if (part.StartsWith("UID:")) {
                            int.TryParse(part.Substring(4), out userId);
                        }
                        if (part.StartsWith("BD:")) {
                            var idsStr = part.Substring(3).Split(',');
                            foreach(var idStr in idsStr) {
                                if (int.TryParse(idStr, out int id)) detailIds.Add(id);
                            }
                        }
                    }
                }

                if (!response.Success || response.VnPayResponseCode != "00")
                {
                    var failUrl = $"{frontendUrl}/Payment/PaymentResult?success=false&message={Uri.EscapeDataString("Giao dịch thất bại hoặc bị hủy.")}";
                    if (userId > 0) failUrl += $"&userId={userId}";
                    return Redirect(failUrl);
                }

                if (userId > 0 && detailIds.Any())
                {
                    // Convert amount from VNPay (which is * 100)
                    string amountStr = Request.Query["vnp_Amount"].ToString();
                    if (decimal.TryParse(amountStr, out decimal vnpAmount))
                    {
                        decimal actualAmount = vnpAmount / 100;
                        await _paymentService.ProcessPaymentAsync(userId, detailIds, actualAmount, PaymentMethod.VnPay);
                    }
                }

                return Redirect($"{frontendUrl}/Payment/PaymentResult?success=true&transactionId={Uri.EscapeDataString(response.TransactionId ?? "")}&userId={userId}");
            }
            catch (Exception ex)
            {
                var frontendUrl = _configuration["ClientUrl"] ?? "https://localhost:7119"; 
                var errorMsg = $"Lỗi xử lý hệ thống: {ex.Message}" + (ex.InnerException != null ? $" | Inner: {ex.InnerException.Message}" : "");
                
                // Try to extract userId from query/order description if possible
                int userId = 0;
                try
                {
                    var orderInfo = Request.Query["vnp_OrderInfo"].ToString();
                    if (!string.IsNullOrEmpty(orderInfo))
                    {
                        var parts = orderInfo.Split('|');
                        foreach(var part in parts) {
                            if (part.StartsWith("UID:")) {
                                int.TryParse(part.Substring(4), out userId);
                            }
                        }
                    }
                }
                catch {}

                var failUrl = $"{frontendUrl}/Payment/PaymentResult?success=false&message={Uri.EscapeDataString(errorMsg)}";
                if (userId > 0) failUrl += $"&userId={userId}";
                return Redirect(failUrl);
            }
        }
    }
}
