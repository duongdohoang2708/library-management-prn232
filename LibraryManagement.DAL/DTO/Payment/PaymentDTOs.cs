namespace LibraryManagementDAL.DTO.Payment
{
    public class UserFinesDTO
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public decimal TotalUnpaidFine { get; set; }
        public int UnpaidBookCount { get; set; }
    }
    
    public class PagedUserFinesResponse
    {
        public List<UserFinesDTO> Users { get; set; } = new List<UserFinesDTO>();
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
    }

    public class ProcessPaymentRequestDTO
    {
        public int UserId { get; set; }
        public List<int> BorrowDetailIds { get; set; } = new List<int>();
        public decimal AmountPaid { get; set; }
        public string PaymentMethod { get; set; } = "Cash"; // Cash, VnPay
    }

    public class CreateVnPayUrlRequestDTO
    {
        public int UserId { get; set; }
        public List<int> BorrowDetailIds { get; set; } = new List<int>();
        public decimal Amount { get; set; }
    }

    public class CreateManualFineRequestDTO
    {
        public int UserId { get; set; }
        public decimal Amount { get; set; }
        public string Reason { get; set; } = string.Empty;
    }
}
