using LibraryManagement.DAL.Data;
using LibraryManagementDAL.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.DAL.Repositories
{
    public class ReservationRepository
    {
        private readonly ApplicationDbContext db;

        public ReservationRepository(ApplicationDbContext _db)
        {
            db = _db;
        }

        public IQueryable<Reservation> QueryReservations()
        {
            return db.Reservations
                .Include(x => x.Account)
                .Include(x => x.Book)
                .Include(x => x.BookCopy)
                .AsQueryable();
        }

        public async Task<Account?> GetAccountAsync(int userId)
        {
            return await db.Accounts
                .Include(x => x.Member)
                .FirstOrDefaultAsync(x => x.UserId == userId && x.IsActive);
        }

        public async Task<Book?> GetBookAsync(int bookId)
        {
            return await db.Books.FirstOrDefaultAsync(x => x.BookId == bookId && x.IsActive);
        }

        public async Task<BookCopy?> GetFirstAvailableCopyAsync(int bookId)
        {
            return await db.BookCopies
                .Where(x => x.BookId == bookId && x.Status == BookCopyStatus.Available)
                .OrderBy(x => x.BookCopyId)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> HasOpenReservationAsync(int userId, int bookId)
        {
            return await db.Reservations.AnyAsync(x =>
                x.UserId == userId &&
                x.BookId == bookId &&
                (x.Status == ReservationStatus.Pending || x.Status == ReservationStatus.Allocated));
        }

        public async Task<int> CountPendingReservationsAsync(int bookId)
        {
            return await db.Reservations.CountAsync(x =>
                x.BookId == bookId &&
                x.Status == ReservationStatus.Pending);
        }

        public IQueryable<Reservation> QueryUserReservations(int userId)
        {
            return QueryReservations()
                .Where(x => x.UserId == userId);
        }

        public async Task<Reservation?> GetReservationAsync(int reservationId)
        {
            return await QueryReservations()
                .FirstOrDefaultAsync(x => x.ReservationId == reservationId);
        }

        public async Task<List<Reservation>> GetPendingReservationsForBookAsync(int bookId)
        {
            return await db.Reservations
                .Include(x => x.Book)
                .Where(x => x.BookId == bookId && x.Status == ReservationStatus.Pending)
                .OrderBy(x => x.ReservedAt)
                .ToListAsync();
        }

        public void AddReservation(Reservation reservation)
        {
            db.Reservations.Add(reservation);
        }

        public void AddTransaction(BorrowTransaction transaction)
        {
            db.BorrowTransactions.Add(transaction);
        }

        public async Task<int> CountOpenBorrowedBooksAsync(int userId)
        {
            return await db.BorrowDetails
                .Where(x =>
                    x.BorrowTransaction.UserId == userId &&
                    x.ActualReturnDate == null)
                .CountAsync();
        }

        public async Task<int> CountOverdueBooksAsync(int userId, DateTime now)
        {
            return await db.BorrowDetails
                .Where(x =>
                    x.BorrowTransaction.UserId == userId &&
                    x.ActualReturnDate == null &&
                    x.DueDate.Date < now.Date)
                .CountAsync();
        }

        public async Task<decimal> GetTotalUnpaidFinesAsync(int userId)
        {
            return await db.BorrowDetails
                .Where(x =>
                    x.BorrowTransaction.UserId == userId &&
                    x.FineAmount > (x.FinePaidAmount ?? 0))
                .SumAsync(x => (x.FineAmount ?? 0) - (x.FinePaidAmount ?? 0));
        }

        public async Task<int> CountAllocatedReservationsAsync(int userId)
        {
            return await db.Reservations
                .Where(x =>
                    x.UserId == userId &&
                    x.Status == ReservationStatus.Allocated)
                .CountAsync();
        }

        public async Task<bool> IsCurrentlyBorrowingBookAsync(int userId, int bookId)
        {
            return await db.BorrowDetails
                .AnyAsync(x => 
                    x.BorrowTransaction.UserId == userId && 
                    x.ActualReturnDate == null && 
                    x.BookCopy.BookId == bookId);
        }

        public async Task SaveChangesAsync()
        {
            await db.SaveChangesAsync();
        }
    }
}
