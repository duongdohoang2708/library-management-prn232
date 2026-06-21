using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace LibraryManagement.BLL.Services.Interface
{
    public interface IVnPayService
    {
        string CreatePaymentUrl(HttpContext context, string transactionRef, decimal amount, string orderInfo);
        VnPayResponseModel PaymentExecute(IQueryCollection collections);
    }

    public class VnPayResponseModel
    {
        public bool Success { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string OrderDescription { get; set; } = string.Empty;
        public string OrderId { get; set; } = string.Empty;
        public string PaymentId { get; set; } = string.Empty;
        public string TransactionId { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string VnPayResponseCode { get; set; } = string.Empty;
    }
}
