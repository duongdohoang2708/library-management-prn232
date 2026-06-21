using LibraryManagementDAL.DTO.Circulation;
using LibraryManagementDAL.DTO.Reservation;
using LibraryManagementDAL.Models;

namespace LibraryManagement.Client.DTO.User
{
    public class MemberDashboardViewModel
    {
        public List<CirculationTransactionItem> BorrowTransactions { get; set; } = new();
        public List<ReservationItem> Reservations { get; set; } = new();
        public List<Notification> Notifications { get; set; } = new();

        public int CurrentlyBorrowedCount { get; set; }
        public int OverdueCount { get; set; }
        public decimal UnpaidFineAmount { get; set; }
        public int ActiveReservationCount { get; set; }
    }
}
