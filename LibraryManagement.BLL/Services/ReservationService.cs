using LibraryManagement.DAL.Repositories;
using LibraryManagement.BLL.DTO.Notification;
using LibraryManagementDAL.DTO.Reservation;
using LibraryManagementDAL.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.BLL.Services
{
    public class ReservationService
    {
        private readonly ReservationRepository reservationRepository;
        private readonly NotificationService notificationService;
        private readonly SystemSettingService systemSettingService;
        private readonly AuditLogService auditLogService;

        public ReservationService(
            ReservationRepository _reservationRepository,
            NotificationService _notificationService,
            SystemSettingService _systemSettingService,
            AuditLogService _auditLogService)
        {
            reservationRepository = _reservationRepository;
            notificationService = _notificationService;
            systemSettingService = _systemSettingService;
            auditLogService = _auditLogService;
        }

        public async Task<List<ReservationItem>> GetReservationsAsync()
        {
            await RefreshReservationsAsync();

            return await reservationRepository.QueryReservations()
                .Where(x => x.Status == ReservationStatus.Pending || x.Status == ReservationStatus.Allocated)
                .OrderByDescending(x => x.Status == ReservationStatus.Allocated)
                .ThenBy(x => x.ReservedAt)
                .Select(x => new ReservationItem
                {
                    ReservationId = x.ReservationId,
                    UserId = x.UserId,
                    UserFullName = x.Account.FullName,
                    UserEmail = x.Account.Email,
                    BookId = x.BookId,
                    BookTitle = x.Book.Title,
                    BookCopyId = x.BookCopyId,
                    Barcode = x.BookCopy == null ? null : x.BookCopy.Barcode,
                    ReservedAt = x.ReservedAt,
                    ExpireAt = x.ExpireAt,
                    Status = x.Status
                })
                .ToListAsync();
        }

        public async Task<List<ReservationItem>> GetUserReservationsAsync(int userId)
        {
            await RefreshReservationsAsync();

            return await reservationRepository.QueryUserReservations(userId)
                .OrderByDescending(x => x.ReservedAt)
                .Select(x => new ReservationItem
                {
                    ReservationId = x.ReservationId,
                    UserId = x.UserId,
                    UserFullName = x.Account.FullName,
                    UserEmail = x.Account.Email,
                    BookId = x.BookId,
                    BookTitle = x.Book.Title,
                    BookCopyId = x.BookCopyId,
                    Barcode = x.BookCopy == null ? null : x.BookCopy.Barcode,
                    ReservedAt = x.ReservedAt,
                    ExpireAt = x.ExpireAt,
                    Status = x.Status
                })
                .ToListAsync();
        }

        public async Task<int> CountPendingReservationsAsync(int bookId)
        {
            return await reservationRepository.CountPendingReservationsAsync(bookId);
        }

        public async Task<ReservationActionResponse> CreateReservationAsync(ReservationCreateRequest request)
        {
            var user = await reservationRepository.GetAccountAsync(request.UserId);
            if (user == null)
            {
                return Fail("Reader does not exist or is inactive.");
            }

            if (user.Member == null)
            {
                return Fail("Only member accounts can reserve books.");
            }

            var book = await reservationRepository.GetBookAsync(request.BookId);
            if (book == null)
            {
                return Fail("Book does not exist or is inactive.");
            }

            if (await reservationRepository.HasOpenReservationAsync(request.UserId, request.BookId))
            {
                return Fail("This reader already has an open reservation for this book.");
            }

            if (await reservationRepository.IsCurrentlyBorrowingBookAsync(request.UserId, request.BookId))
            {
                return Fail("You cannot reserve this book because you are currently borrowing a copy of it.");
            }

            var now = DateTime.UtcNow;
            var policy = await systemSettingService.GetPolicyAsync();

            var overdueCount = await reservationRepository.CountOverdueBooksAsync(request.UserId, now);
            if (overdueCount > 0)
            {
                return Fail($"You cannot reserve books because you have {overdueCount} overdue book(s) that must be returned first.");
            }

            var unpaidFines = await reservationRepository.GetTotalUnpaidFinesAsync(request.UserId);
            if (unpaidFines > 0)
            {
                return Fail($"You cannot reserve books because you have unpaid fines of {unpaidFines:N0} đ. Fines must be paid off first.");
            }

            var openBorrowedBooks = await reservationRepository.CountOpenBorrowedBooksAsync(request.UserId);
            var allocatedReservations = await reservationRepository.CountAllocatedReservationsAsync(request.UserId);
            if (openBorrowedBooks + allocatedReservations >= policy.MaxOpenBorrowedBooks)
            {
                return Fail($"You cannot reserve more books. You have reached your limit of {policy.MaxOpenBorrowedBooks} active borrowed book(s)/allocated reservation(s).");
            }
            var availableCopy = await reservationRepository.GetFirstAvailableCopyAsync(request.BookId);
            var queuePosition = await reservationRepository.CountPendingReservationsAsync(request.BookId) + 1;
            var reservation = new Reservation
            {
                UserId = request.UserId,
                BookId = request.BookId,
                ReservedAt = now,
                CreatedAt = now,
                Status = ReservationStatus.Pending
            };

            if (availableCopy != null)
            {
                availableCopy.Status = BookCopyStatus.Reserved;
                availableCopy.UpdatedAt = now;
                reservation.BookCopyId = availableCopy.BookCopyId;
                reservation.Status = ReservationStatus.Allocated;
                reservation.ExpireAt = now.AddDays(policy.ReservationHoldDays);
            }

            reservationRepository.AddReservation(reservation);
            await reservationRepository.SaveChangesAsync();

            if (reservation.Status == ReservationStatus.Allocated)
            {
                await NotifyReservationAllocatedAsync(reservation);
            }
            await auditLogService.LogAsync("CreateReservation", "Reservation", reservation.ReservationId.ToString(), $"Reservation created for user #{request.UserId} and book #{request.BookId}.");

            return new ReservationActionResponse
            {
                IsSuccess = true,
                Message = reservation.Status == ReservationStatus.Allocated
                    ? "Your reservation has been placed. A copy is ready for pickup."
                    : $"Your reservation has been placed. You are number {queuePosition} in the queue.",
                ReservationId = reservation.ReservationId,
                QueuePosition = reservation.Status == ReservationStatus.Pending ? queuePosition : 0
            };
        }

        public async Task<ReservationActionResponse> ApproveAsync(int reservationId)
        {
            await RefreshReservationsAsync();

            var reservation = await reservationRepository.GetReservationAsync(reservationId);
            if (reservation == null)
            {
                return Fail("Reservation not found.");
            }

            if (reservation.Status != ReservationStatus.Allocated || reservation.BookCopyId == null || reservation.BookCopy == null)
            {
                return Fail("Only allocated reservations can be approved.");
            }

            if (reservation.BookCopy.Status != BookCopyStatus.Reserved)
            {
                return Fail("The reserved copy is no longer available for this reservation.");
            }

            var now = DateTime.UtcNow;

            var overdueCount = await reservationRepository.CountOverdueBooksAsync(reservation.UserId, now);
            if (overdueCount > 0)
            {
                return Fail($"This reader cannot pick up books because they have {overdueCount} overdue book(s) that must be returned first.");
            }

            var unpaidFines = await reservationRepository.GetTotalUnpaidFinesAsync(reservation.UserId);
            if (unpaidFines > 0)
            {
                return Fail($"This reader cannot pick up books because they have unpaid fines of {unpaidFines:N0} đ. Fines must be paid off first.");
            }

            var policy = await systemSettingService.GetPolicyAsync();
            var openBorrowedBooks = await reservationRepository.CountOpenBorrowedBooksAsync(reservation.UserId);
            if (openBorrowedBooks >= policy.MaxOpenBorrowedBooks)
            {
                return Fail($"This reader cannot borrow more books because they have reached their limit of {policy.MaxOpenBorrowedBooks} borrowed books.");
            }

            if (await reservationRepository.IsCurrentlyBorrowingBookAsync(reservation.UserId, reservation.BookId))
            {
                return Fail("This reader is already borrowing another copy of the same book.");
            }

            var dueDate = now.Date.AddDays(policy.DefaultLoanDays);

            reservation.Status = ReservationStatus.Completed;
            reservation.UpdatedAt = now;
            reservation.BookCopy.Status = BookCopyStatus.Borrowed;
            reservation.BookCopy.UpdatedAt = now;

            var transaction = new BorrowTransaction
            {
                UserId = reservation.UserId,
                BorrowDate = now,
                DueDate = dueDate,
                Status = "Borrowing",
                CreatedAt = now,
                BorrowDetails = new List<BorrowDetail>
                {
                    new BorrowDetail
                    {
                        BookCopyId = reservation.BookCopyId.Value,
                        BorrowDate = now,
                        DueDate = dueDate,
                        FineAmount = 0,
                        FinePaidAmount = 0,
                        IsFinePaid = true,
                        CreatedAt = now
                    }
                }
            };

            reservationRepository.AddTransaction(transaction);
            await reservationRepository.SaveChangesAsync();
            await auditLogService.LogAsync("ApproveReservation", "Reservation", reservation.ReservationId.ToString(), $"Reservation #{reservation.ReservationId} approved and transaction #{transaction.BorrowTransactionId} created.");

            // Send borrow notification
            try
            {
                var bookTitle = reservation.Book?.Title ?? "Unknown Book";
                var dueDateStr = dueDate.ToLocalTime().ToString("dd/MM/yyyy");
                await notificationService.CreateAsync(new NotificationRequest
                {
                    UserId = reservation.UserId,
                    Title = "Books Checked Out",
                    Message = $"You have borrowed: \"{bookTitle}\". Please return it by {dueDateStr}.",
                    Type = "Borrow"
                });
            }
            catch (Exception ex)
            {
                await auditLogService.LogAsync("BorrowNotificationError", "Reservation", reservation.ReservationId.ToString(), $"Failed to send borrow notification on reservation approval: {ex.Message}");
            }

            return new ReservationActionResponse
            {
                IsSuccess = true,
                Message = "Reservation approved and borrow transaction created.",
                ReservationId = reservation.ReservationId,
                TransactionId = transaction.BorrowTransactionId
            };
        }

        public async Task<ReservationActionResponse> CancelAsync(int reservationId)
        {
            var reservation = await reservationRepository.GetReservationAsync(reservationId);
            if (reservation == null)
            {
                return Fail("Reservation not found.");
            }

            if (reservation.Status == ReservationStatus.Completed)
            {
                return Fail("Completed reservations cannot be cancelled.");
            }

            if (reservation.Status == ReservationStatus.Cancelled || reservation.Status == ReservationStatus.Expired)
            {
                return Fail("This reservation is already closed.");
            }

            var now = DateTime.UtcNow;
            var bookId = reservation.BookId;
            reservation.Status = ReservationStatus.Cancelled;
            reservation.UpdatedAt = now;

            if (reservation.BookCopy != null && reservation.BookCopy.Status == BookCopyStatus.Reserved)
            {
                reservation.BookCopy.Status = BookCopyStatus.Available;
                reservation.BookCopy.UpdatedAt = now;
                reservation.BookCopyId = null;
            }

            await reservationRepository.SaveChangesAsync();
            await AllocatePendingReservationsAsync(bookId, now);
            await reservationRepository.SaveChangesAsync();
            await auditLogService.LogAsync("CancelReservation", "Reservation", reservation.ReservationId.ToString(), $"Reservation #{reservation.ReservationId} cancelled.");

            return new ReservationActionResponse
            {
                IsSuccess = true,
                Message = "Reservation cancelled.",
                ReservationId = reservation.ReservationId
            };
        }

        private async Task RefreshReservationsAsync()
        {
            var now = DateTime.UtcNow;
            var allocatedWithoutCopy = await reservationRepository.QueryReservations()
                .Where(x => x.Status == ReservationStatus.Allocated && x.BookCopyId == null)
                .ToListAsync();

            foreach (var reservation in allocatedWithoutCopy)
            {
                reservation.Status = ReservationStatus.Pending;
                reservation.UpdatedAt = now;
            }

            var allocated = await reservationRepository.QueryReservations()
                .Where(x =>
                    x.Status == ReservationStatus.Allocated &&
                    x.ExpireAt != null &&
                    x.ExpireAt < now)
                .ToListAsync();

            var touchedBookIds = new HashSet<int>();
            foreach (var reservation in allocated)
            {
                reservation.Status = ReservationStatus.Expired;
                reservation.UpdatedAt = now;
                touchedBookIds.Add(reservation.BookId);

                if (reservation.BookCopy != null && reservation.BookCopy.Status == BookCopyStatus.Reserved)
                {
                    reservation.BookCopy.Status = BookCopyStatus.Available;
                    reservation.BookCopy.UpdatedAt = now;
                    reservation.BookCopyId = null;
                }
            }

            if (allocatedWithoutCopy.Any() || allocated.Any())
            {
                await reservationRepository.SaveChangesAsync();
            }

            var pendingBookIds = await reservationRepository.QueryReservations()
                .Where(x => x.Status == ReservationStatus.Pending)
                .Select(x => x.BookId)
                .Distinct()
                .ToListAsync();

            foreach (var bookId in pendingBookIds.Union(touchedBookIds))
            {
                await AllocatePendingReservationsAsync(bookId, now);
            }

            if (pendingBookIds.Any())
            {
                await reservationRepository.SaveChangesAsync();
            }
        }

        private async Task AllocatePendingReservationsAsync(int bookId, DateTime now)
        {
            var policy = await systemSettingService.GetPolicyAsync();
            var pendingReservations = await reservationRepository.GetPendingReservationsForBookAsync(bookId);

            foreach (var reservation in pendingReservations)
            {
                var availableCopy = await reservationRepository.GetFirstAvailableCopyAsync(bookId);
                if (availableCopy == null)
                {
                    break;
                }

                availableCopy.Status = BookCopyStatus.Reserved;
                availableCopy.UpdatedAt = now;
                reservation.BookCopyId = availableCopy.BookCopyId;
                reservation.Status = ReservationStatus.Allocated;
                reservation.ExpireAt = now.AddDays(policy.ReservationHoldDays);
                reservation.UpdatedAt = now;
                await NotifyReservationAllocatedAsync(reservation);
            }
        }

        public async Task AllocatePendingReservationsForBooksAsync(IEnumerable<int> bookIds)
        {
            var now = DateTime.UtcNow;
            foreach (var bookId in bookIds.Distinct())
            {
                await AllocatePendingReservationsAsync(bookId, now);
            }

            await reservationRepository.SaveChangesAsync();
        }

        private async Task NotifyReservationAllocatedAsync(Reservation reservation)
        {
            var bookTitle = reservation.Book?.Title ?? "your reserved book";
            var expireText = reservation.ExpireAt?.ToLocalTime().ToString("dd/MM/yyyy HH:mm") ?? "soon";
            await notificationService.CreateAsync(new NotificationRequest
            {
                UserId = reservation.UserId,
                Title = "Reserved book is ready",
                Message = $"Your reserved book \"{bookTitle}\" is ready for pickup. Please collect it before {expireText}.",
                Type = "Reservation"
            });
        }

        private static ReservationActionResponse Fail(string message)
        {
            return new ReservationActionResponse
            {
                IsSuccess = false,
                Message = message
            };
        }
    }
}
