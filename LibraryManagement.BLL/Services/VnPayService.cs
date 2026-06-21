using LibraryManagement.BLL.Services.Interface;
using LibraryManagement.BLL.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;

namespace LibraryManagement.BLL.Services
{
    public class VnPayService : IVnPayService
    {
        private readonly IConfiguration _configuration;

        public VnPayService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string CreatePaymentUrl(HttpContext context, string transactionRef, decimal amount, string orderInfo)
        {
            var tick = DateTime.Now.Ticks.ToString();
            
            var vnpay = new VnPayLibrary();
            vnpay.AddRequestData("vnp_Version", _configuration["Vnpay:Version"] ?? "");
            vnpay.AddRequestData("vnp_Command", _configuration["Vnpay:Command"] ?? "");
            vnpay.AddRequestData("vnp_TmnCode", _configuration["Vnpay:TmnCode"] ?? "");
            // Multiply by 100 per VNPay requirement
            vnpay.AddRequestData("vnp_Amount", (amount * 100).ToString("0"));
            
            vnpay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
            vnpay.AddRequestData("vnp_CurrCode", _configuration["Vnpay:CurrCode"] ?? "");
            vnpay.AddRequestData("vnp_IpAddr", Utils.GetIpAddress(context));
            vnpay.AddRequestData("vnp_Locale", _configuration["Vnpay:Locale"] ?? "");
            
            vnpay.AddRequestData("vnp_OrderInfo", orderInfo);
            vnpay.AddRequestData("vnp_OrderType", "other");
            
            // Wait, the API doesn't know the Client's domain.
            // We should get the Return URL from configuration or construct it based on the Client application
            // In a split architecture, the Return URL is usually the API's return endpoint.
            var returnUrl = _configuration["Vnpay:ReturnUrl"];
            if (string.IsNullOrEmpty(returnUrl))
            {
                // Fallback to current request host (useful if monolithic, but here API and Client are split)
                // Assuming the API handles the return and redirects to client
                returnUrl = $"{context.Request.Scheme}://{context.Request.Host}/api/payments/vnpay-return";
            }
            vnpay.AddRequestData("vnp_ReturnUrl", returnUrl);
            vnpay.AddRequestData("vnp_TxnRef", transactionRef);

            var paymentUrl = vnpay.CreateRequestUrl(_configuration["Vnpay:BaseUrl"] ?? "", _configuration["Vnpay:HashSecret"] ?? "");
            return paymentUrl;
        }

        public VnPayResponseModel PaymentExecute(IQueryCollection collections)
        {
            var vnpay = new VnPayLibrary();
            foreach (var (key, value) in collections)
            {
                if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_"))
                {
                    vnpay.AddResponseData(key, value.ToString());
                }
            }

            var vnp_orderId = vnpay.GetResponseData("vnp_TxnRef");
            var vnp_TransactionId = vnpay.GetResponseData("vnp_TransactionNo");
            var vnp_SecureHash = collections.FirstOrDefault(p => p.Key == "vnp_SecureHash").Value.ToString();
            var vnp_ResponseCode = vnpay.GetResponseData("vnp_ResponseCode");
            var vnp_OrderInfo = vnpay.GetResponseData("vnp_OrderInfo");

            bool checkSignature = vnpay.ValidateSignature(vnp_SecureHash, _configuration["Vnpay:HashSecret"] ?? "");
            
            if (!checkSignature)
            {
                return new VnPayResponseModel
                {
                    Success = false
                };
            }

            return new VnPayResponseModel
            {
                Success = true,
                PaymentMethod = "VnPay",
                OrderDescription = vnp_OrderInfo,
                OrderId = vnp_orderId,
                TransactionId = vnp_TransactionId,
                Token = vnp_SecureHash,
                VnPayResponseCode = vnp_ResponseCode
            };
        }
    }
}
