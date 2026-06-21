using LibraryManagementDAL.DTO.Payment;
using LibraryManagement.Client.Helpers;
using LibraryManagementDAL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;

namespace LibraryManagement.Client.Controllers
{
    public class VnPayUrlResponse
    {
        public bool success { get; set; }
        public string url { get; set; }
    }

    public class CashPaymentResponse
    {
        public bool success { get; set; }
        public string transactionCode { get; set; }
    }

    [Authorize]
    public class PaymentController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public PaymentController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        [Authorize(Roles = "Admin,Librarian")]
        public async Task<IActionResult> Index(int page = 1, string? search = null)
        {
            var client = _httpClientFactory.CreateClient();
            ApiActorHeaderHelper.AddActorHeaders(client, User);

            var query = new List<string> { $"page={page}" };
            if (!string.IsNullOrEmpty(search)) query.Add($"search={Uri.EscapeDataString(search)}");

            var url = $"{GetApiBaseUrl()}/api/payments/users-with-fines?{string.Join("&", query)}";
            var result = await client.GetFromJsonAsync<PagedUserFinesResponse>(url) ?? new PagedUserFinesResponse();

            ViewBag.SearchQuery = search;
            ViewBag.CurrentPage = result.CurrentPage;
            ViewBag.TotalPages = result.TotalPages;
            
            return View(result.Users);
        }

        [Authorize(Roles = "Admin,Librarian")]
        public async Task<IActionResult> Details(int id)
        {
            var client = _httpClientFactory.CreateClient();
            ApiActorHeaderHelper.AddActorHeaders(client, User);

            var url = $"{GetApiBaseUrl()}/api/payments/user/{id}";
            var fines = await client.GetFromJsonAsync<List<BorrowDetail>>(url);

            if (fines == null || !fines.Any())
            {
                if (!TempData.ContainsKey("SuccessMessage") && !TempData.ContainsKey("ErrorMessage"))
                {
                    TempData["SuccessMessage"] = "Người dùng này không có khoản phạt nào cần thanh toán.";
                }
                TempData.Keep();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.UserId = id;
            var firstFine = fines.First();
            ViewBag.FullName = firstFine.BorrowTransaction?.Account?.FullName;
            ViewBag.Email = firstFine.BorrowTransaction?.Account?.Email;
            
            return View(fines);
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Librarian")]
        public async Task<IActionResult> SearchUsers(string query)
        {
            if (string.IsNullOrWhiteSpace(query)) return Json(new List<object>());
            var client = _httpClientFactory.CreateClient();
            ApiActorHeaderHelper.AddActorHeaders(client, User);
            var url = $"{GetApiBaseUrl()}/api/circulation/users?query={Uri.EscapeDataString(query)}";
            var result = await client.GetFromJsonAsync<object>(url);
            return Json(result);
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Librarian")]
        public IActionResult CreateFine()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Librarian")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateFine(int userId, decimal amount, string reason)
        {
            try
            {
                if (amount <= 0) throw new Exception("Số tiền phạt phải lớn hơn 0.");
                if (string.IsNullOrWhiteSpace(reason)) throw new Exception("Vui lòng nhập lý do phạt.");

                var client = _httpClientFactory.CreateClient();
                ApiActorHeaderHelper.AddActorHeaders(client, User);

                var request = new CreateManualFineRequestDTO
                {
                    UserId = userId,
                    Amount = amount,
                    Reason = reason
                };

                var response = await client.PostAsJsonAsync($"{GetApiBaseUrl()}/api/payments/manual-fine", request);
                
                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Đã tạo khoản phạt thủ công thành công!";
                    return RedirectToAction(nameof(Index));
                }
                
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Lỗi từ API: {error}");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return View();
            }
        }

        public async Task<IActionResult> MyFines()
        {
            var client = _httpClientFactory.CreateClient();
            ApiActorHeaderHelper.AddActorHeaders(client, User);

            var url = $"{GetApiBaseUrl()}/api/payments/my-fines";
            var result = await client.GetFromJsonAsync<List<BorrowDetail>>(url);

            return View(result ?? new List<BorrowDetail>());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Process(int userId, List<int> selectedFines, decimal amount, string method)
        {
            if (selectedFines == null || !selectedFines.Any() || amount <= 0)
            {
                TempData["ErrorMessage"] = "Dữ liệu không hợp lệ. Vui lòng kiểm tra lại số tiền và các khoản phạt được chọn.";
                return RedirectToAction(nameof(Details), new { id = userId });
            }

            if (method == "VnPay")
            {
                var request = new CreateVnPayUrlRequestDTO
                {
                    UserId = userId,
                    BorrowDetailIds = selectedFines,
                    Amount = amount
                };
                
                var client = _httpClientFactory.CreateClient();
                ApiActorHeaderHelper.AddActorHeaders(client, User);

                var response = await client.PostAsJsonAsync($"{GetApiBaseUrl()}/api/payments/vnpay-url", request);
                
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<VnPayUrlResponse>();
                    string url = result?.url ?? "";
                    if (!string.IsNullOrEmpty(url))
                    {
                        return Redirect(url);
                    }
                }
                
                return RedirectToAction("PaymentResult", new { success = false, message = "Could not generate VnPay URL" });
            }

            // Cash Processing
            try
            {
                var cashRequest = new ProcessPaymentRequestDTO
                {
                    UserId = userId,
                    BorrowDetailIds = selectedFines,
                    AmountPaid = amount
                };
                
                var client = _httpClientFactory.CreateClient();
                ApiActorHeaderHelper.AddActorHeaders(client, User);

                var response = await client.PostAsJsonAsync($"{GetApiBaseUrl()}/api/payments/process-cash", cashRequest);
                
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<CashPaymentResponse>();
                    string transCode = result?.transactionCode ?? "";
                    return RedirectToAction("PaymentResult", new { success = true, transactionId = transCode, userId = userId });
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    string errorMsg = error;
                    try
                    {
                        var errObj = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.Nodes.JsonObject>(error);
                        if (errObj != null && errObj.TryGetPropertyValue("message", out var msgNode))
                        {
                            errorMsg = msgNode?.ToString() ?? error;
                        }
                    }
                    catch {}
                    return RedirectToAction("PaymentResult", new { success = false, message = errorMsg, userId = userId });
                }
            }
            catch (Exception ex)
            {
                return RedirectToAction("PaymentResult", new { success = false, message = ex.Message, userId = userId });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Librarian")]
        public async Task<IActionResult> ProcessCash([FromBody] ProcessPaymentRequestDTO request)
        {
            var client = _httpClientFactory.CreateClient();
            ApiActorHeaderHelper.AddActorHeaders(client, User);

            var response = await client.PostAsJsonAsync($"{GetApiBaseUrl()}/api/payments/process-cash", request);
            
            if (response.IsSuccessStatusCode)
            {
                return Json(new { success = true });
            }
            return BadRequest(await response.Content.ReadAsStringAsync());
        }

        [HttpGet]
        public IActionResult PaymentResult(bool success, string transactionId, string message, int? userId)
        {
            ViewBag.Success = success;
            ViewBag.TransactionId = transactionId;
            ViewBag.Message = message;
            ViewBag.TargetUserId = userId;
            return View();
        }

        private string GetApiBaseUrl()
        {
            return _configuration["ApiSettings:BaseUrl"]?.TrimEnd('/')
                ?? throw new InvalidOperationException("ApiSettings:BaseUrl is not configured.");
        }
    }
}
