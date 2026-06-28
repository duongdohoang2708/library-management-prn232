using LibraryManagement.DAL.Repositories;
using LibraryManagementDAL.DTO.Circulation;
using LibraryManagementDAL.Models;
using Microsoft.EntityFrameworkCore;
using LibraryManagement.BLL.DTO.Notification;

namespace LibraryManagement.BLL.Services
{
    public class CirculationService
    {
        private const int PageSize = 10;
        private readonly CirculationRepository circulationRepository;
        private readonly NotificationService notificationService;
        private readonly ReservationService reservationService;
        private readonly SystemSettingService systemSettingService;
        private readonly AuditLogService auditLogService;

        public CirculationService(
            CirculationRepository _circulationRepository,
            ReservationService _reservationService,
            SystemSettingService _systemSettingService,
            AuditLogService _auditLogService,
            NotificationService _notificationService)
        {
            circulationRepository = _circulationRepository;
            reservationService = _reservationService;
            systemSettingService = _systemSettingService;
            auditLogService = _auditLogService;
            notificationService = _notificationService;
        }

        public async Task<CirculationListResult> GetTransactionsAsync(
            string? searchQuery,
            string? statusFilter,
            int page)
        {
            await RefreshOverdueTransactionsAsync();

            page = page < 1 ? 1 : page;
            var query = circulationRepository.QueryTransactions();

            if (!string.IsNullOrWhiteSpace(statusFilter))
            {
                query = query.Where(x => x.Status == statusFilter);
            }

            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                var key = searchQuery.Trim();
                query = query.Where(x =>
                    x.BorrowTransactionId.ToString().Contains(key) ||
                    x.UserId.ToString().Contains(key) ||
                    x.Account.FullName.Contains(key) ||
                    x.Account.Email.Contains(key) ||
                    x.BorrowDetails.Any(d =>
                        d.BookCopy != null &&
                        (d.BookCopy.Barcode.Contains(key) ||
                         d.BookCopy.Book.Title.Contains(key))));
            }

            query = query
                .OrderByDescending(x => x.BorrowDate)
                .ThenByDescending(x => x.BorrowTransactionId);

            var totalItems = await query.CountAsync();
            var totalPages = Math.Max(1, (int)Math.Ceiling(totalItems / (double)PageSize));
            page = Math.Min(page, totalPages);

            return new CirculationListResult
            {
                Items = (await query.Skip((page - 1) * PageSize).Take(PageSize).ToListAsync())
                    .Select(MapTransaction)
                    .ToList(),
                CurrentPage = page,
                TotalPages = totalPages
            };
        }

        public async Task<List<UserSearchResult>> SearchUsersAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return new List<UserSearchResult>();
            }

            var accounts = await circulationRepository.SearchAccountsAsync(query);
            return accounts.Select(x => new UserSearchResult
            {
                UserId = x.UserId,
                FullName = x.FullName,
                Email = x.Email
            }).ToList();
        }

        public async Task<List<AvailableCopyResult>> GetAvailableCopiesAsync()
        {
            var copies = await circulationRepository.GetAvailableCopiesAsync();
            return copies.Select(copy => new AvailableCopyResult
            {
                Barcode = copy.Barcode,
                BookTitle = copy.Book?.Title ?? string.Empty,
                AuthorName = copy.Book?.Author?.Name ?? string.Empty,
                Location = copy.Location
            }).ToList();
        }

        public async Task<List<CirculationTransactionItem>> GetUserBorrowHistoryAsync(int userId)
        {
            await RefreshOverdueTransactionsAsync();

            var transactions = await circulationRepository.QueryTransactions()
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.BorrowDate)
                .ToListAsync();

            return transactions.Select(MapTransaction).ToList();
        }

        public async Task<CirculationActionResponse> BorrowAsync(BorrowRequest request)
        {
            var user = await circulationRepository.GetAccountAsync(request.UserId);
            if (user == null)
            {
                return Fail("Reader does not exist or is inactive.");
            }

            var barcodes = request.Barcodes
                .Select(x => x.Trim())
                .Where(x => x != string.Empty)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (!barcodes.Any())
            {
                return Fail("Please select at least one book.");
            }

            var copies = await circulationRepository.GetCopiesByBarcodesAsync(barcodes);
            if (copies.Count != barcodes.Count)
            {
                return Fail("One or more selected books could not be found.");
            }

            var unavailableCopy = copies.FirstOrDefault(x => x.Status != BookCopyStatus.Available);
            if (unavailableCopy != null)
            {
                return Fail($"Book copy {unavailableCopy.Barcode} is not available.");
            }

            var policy = await systemSettingService.GetPolicyAsync();
            var now = DateTime.UtcNow;

            var overdueCount = await circulationRepository.CountOverdueBooksAsync(user.UserId, now);
            if (overdueCount > 0)
            {
                return Fail($"This reader cannot borrow books because they have {overdueCount} overdue book(s) that must be returned first.");
            }

            var unpaidFines = await circulationRepository.GetTotalUnpaidFinesAsync(user.UserId);
            if (unpaidFines > 0)
            {
                return Fail($"This reader cannot borrow books because they have unpaid fines of {unpaidFines:N0} đ. Fines must be paid off first.");
            }

            // Check if there are multiple copies of the same book in this checkout request
            var bookIdsInRequest = copies.Select(x => x.BookId).ToList();
            if (bookIdsInRequest.Distinct().Count() != bookIdsInRequest.Count)
            {
                return Fail("This reader cannot borrow multiple copies of the same book in the same request.");
            }

            // Check if user is already borrowing any of these books
            foreach (var copy in copies)
            {
                var isBorrowing = await circulationRepository.IsCurrentlyBorrowingBookAsync(user.UserId, copy.BookId);
                if (isBorrowing)
                {
                    return Fail($"This reader is already borrowing a copy of \"{copy.Book?.Title ?? "this book"}\". They cannot borrow another copy of the same book.");
                }
            }

            var openBorrowedBooks = await circulationRepository.CountOpenBorrowedBooksAsync(user.UserId);
            if (openBorrowedBooks + barcodes.Count > policy.MaxOpenBorrowedBooks)
            {
                return Fail($"This reader can borrow at most {policy.MaxOpenBorrowedBooks} books. They currently have {openBorrowedBooks} borrowed books, and are trying to borrow {barcodes.Count} more.");
            }

            var dueDate = now.Date.AddDays(request.LoanDays <= 0 ? policy.DefaultLoanDays : request.LoanDays);

            var transaction = new BorrowTransaction
            {
                UserId = user.UserId,
                BorrowDate = now,
                DueDate = dueDate,
                Status = "Borrowing",
                CreatedAt = now,
                BorrowDetails = copies.Select(copy =>
                {
                    copy.Status = BookCopyStatus.Borrowed;
                    copy.UpdatedAt = now;

                    return new BorrowDetail
                    {
                        BookCopyId = copy.BookCopyId,
                        BorrowDate = now,
                        DueDate = dueDate,
                        FineAmount = 0,
                        FinePaidAmount = 0,
                        IsFinePaid = true,
                        CreatedAt = now
                    };
                }).ToList()
            };

            circulationRepository.AddTransaction(transaction);
            await circulationRepository.SaveChangesAsync();
            await auditLogService.LogAsync("BorrowBooks", "BorrowTransaction", transaction.BorrowTransactionId.ToString(), $"Borrow transaction created for user #{user.UserId} with {copies.Count} copies.");

            // Send borrow notification to member
            try
            {
                var bookTitles = string.Join(", ", copies.Select(x => $"\"{x.Book?.Title ?? "Unknown Book"}\""));
                var dueDateStr = dueDate.ToLocalTime().ToString("dd/MM/yyyy");
                await notificationService.CreateAsync(new NotificationRequest
                {
                    UserId = user.UserId,
                    Title = "Books Checked Out",
                    Message = $"You have borrowed: {bookTitles}. Please return them by {dueDateStr}.",
                    Type = "Borrow"
                });
            }
            catch (Exception ex)
            {
                await auditLogService.LogAsync("BorrowNotificationError", "BorrowTransaction", transaction.BorrowTransactionId.ToString(), $"Failed to send borrow notification: {ex.Message}");
            }

            return new CirculationActionResponse
            {
                IsSuccess = true,
                Message = "Borrow transaction created successfully.",
                TransactionId = transaction.BorrowTransactionId
            };
        }

        public async Task<CirculationActionResponse> MemberBorrowNowAsync(MemberBorrowRequest request)
        {
            var user = await circulationRepository.GetAccountAsync(request.UserId);
            if (user == null)
            {
                return Fail("Your account does not exist or is inactive.");
            }

            var bookIds = request.BookIds.Distinct().ToList();
            if (!bookIds.Any())
            {
                return Fail("Please select at least one book.");
            }

            var policy = await systemSettingService.GetPolicyAsync();
            var now = DateTime.UtcNow;

            // Validate no overdue books
            var overdueCount = await circulationRepository.CountOverdueBooksAsync(user.UserId, now);
            if (overdueCount > 0)
            {
                return Fail($"You cannot borrow books because you have {overdueCount} overdue book(s) that must be returned first.");
            }

            // Validate no unpaid fines
            var unpaidFines = await circulationRepository.GetTotalUnpaidFinesAsync(user.UserId);
            if (unpaidFines > 0)
            {
                return Fail($"You cannot borrow books because you have unpaid fines of {unpaidFines:N0} đ. Please pay your fines first.");
            }

            // Validate max open borrowed books
            var openBorrowedBooks = await circulationRepository.CountOpenBorrowedBooksAsync(user.UserId);
            if (openBorrowedBooks + bookIds.Count > policy.MaxOpenBorrowedBooks)
            {
                return Fail($"You can borrow at most {policy.MaxOpenBorrowedBooks} books total. You currently have {openBorrowedBooks} borrowed.");
            }

            // Find available copies for each BookId
            var copies = new List<BookCopy>();
            foreach (var bookId in bookIds)
            {
                // Check not already borrowing this book
                var isBorrowing = await circulationRepository.IsCurrentlyBorrowingBookAsync(user.UserId, bookId);
                if (isBorrowing)
                {
                    return Fail($"You are already borrowing a copy of one of the selected books.");
                }

                var copy = await circulationRepository.GetFirstAvailableCopyByBookIdAsync(bookId);
                if (copy == null)
                {
                    return Fail($"One of the selected books has no available copies. Please try again or reserve it instead.");
                }
                copies.Add(copy);
            }

            var dueDate = now.Date.AddDays(policy.DefaultLoanDays);

            var transaction = new BorrowTransaction
            {
                UserId = user.UserId,
                BorrowDate = now,
                DueDate = dueDate,
                Status = "Borrowing",
                CreatedAt = now,
                BorrowDetails = copies.Select(copy =>
                {
                    copy.Status = BookCopyStatus.Borrowed;
                    copy.UpdatedAt = now;
                    return new BorrowDetail
                    {
                        BookCopyId = copy.BookCopyId,
                        BorrowDate = now,
                        DueDate = dueDate,
                        FineAmount = 0,
                        FinePaidAmount = 0,
                        IsFinePaid = true,
                        CreatedAt = now
                    };
                }).ToList()
            };

            circulationRepository.AddTransaction(transaction);
            await circulationRepository.SaveChangesAsync();
            await auditLogService.LogAsync("MemberBorrowBooks", "BorrowTransaction", transaction.BorrowTransactionId.ToString(),
                $"Member #{user.UserId} self-checked out {copies.Count} book(s).");

            // Send notification
            try
            {
                var bookTitles = string.Join(", ", copies.Select(x => $"\"{x.Book?.Title ?? "Unknown Book"}\""));
                var dueDateStr = dueDate.ToLocalTime().ToString("dd/MM/yyyy");
                await notificationService.CreateAsync(new NotificationRequest
                {
                    UserId = user.UserId,
                    Title = "Books Checked Out",
                    Message = $"You have borrowed: {bookTitles}. Please return by {dueDateStr}.",
                    Type = "Borrow"
                });
            }
            catch { }

            return new CirculationActionResponse
            {
                IsSuccess = true,
                Message = $"Successfully checked out {copies.Count} book(s). Due date: {dueDate.ToLocalTime():dd/MM/yyyy}.",
                TransactionId = transaction.BorrowTransactionId
            };
        }


        public async Task<ReturnDetailsResult?> GetReturnDetailsAsync(int transactionId)
        {
            await RefreshOverdueTransactionsAsync();

            var transaction = await circulationRepository.GetTransactionAsync(transactionId);
            if (transaction == null)
            {
                return null;
            }

            return new ReturnDetailsResult
            {
                TransactionId = transaction.BorrowTransactionId,
                UserId = transaction.UserId,
                UserFullName = transaction.Account?.FullName ?? string.Empty,
                UnreturnedDetails = transaction.BorrowDetails
                    .Where(x => x.ActualReturnDate == null)
                    .OrderBy(x => x.DueDate)
                    .Select(MapDetail)
                    .ToList()
            };
        }

        public async Task<CirculationActionResponse> ReturnAsync(int transactionId, ReturnRequest request)
        {
            var transaction = await circulationRepository.GetTransactionAsync(transactionId);
            if (transaction == null)
            {
                return Fail("Borrow transaction not found.");
            }

            var selectedIds = request.BorrowDetailIds.Distinct().ToList();
            if (!selectedIds.Any())
            {
                return Fail("Please select at least one book to return.");
            }

            var policy = await systemSettingService.GetPolicyAsync();
            var now = DateTime.UtcNow;
            var selectedDetails = transaction.BorrowDetails
                .Where(x => selectedIds.Contains(x.BorrowDetailId))
                .ToList();

            if (selectedDetails.Count != selectedIds.Count)
            {
                return Fail("One or more selected books do not belong to this transaction.");
            }

            if (selectedDetails.Any(x => x.ActualReturnDate != null))
            {
                return Fail("One or more selected books were already returned.");
            }

            var returnedBookIds = selectedDetails
                .Where(x => x.BookCopy != null)
                .Select(x => x.BookCopy!.BookId)
                .Distinct()
                .ToList();

            foreach (var detail in selectedDetails)
            {
                detail.ActualReturnDate = now;
                detail.UpdatedAt = now;

                var lateDays = Math.Max(0, (now.Date - detail.DueDate.Date).Days);
                var overdueFine = lateDays * policy.OverdueFinePerDay;
                detail.FineAmount = Math.Max(detail.FineAmount ?? 0, overdueFine);
                detail.IsFinePaid = detail.FineAmount.GetValueOrDefault() == 0;

                if (detail.BookCopy != null)
                {
                    detail.BookCopy.Status = BookCopyStatus.Available;
                    detail.BookCopy.UpdatedAt = now;
                }
            }

            UpdateTransactionStatus(transaction, now);
            await circulationRepository.SaveChangesAsync();
            await reservationService.AllocatePendingReservationsForBooksAsync(returnedBookIds);
            await auditLogService.LogAsync("ReturnBooks", "BorrowTransaction", transaction.BorrowTransactionId.ToString(), $"{selectedDetails.Count} books returned for transaction #{transaction.BorrowTransactionId}.");

            // Send return notification to member
            try
            {
                var returnedBookTitles = string.Join(", ", selectedDetails.Select(x => $"\"{x.BookCopy?.Book?.Title ?? "Unknown Book"}\""));
                var lateDetails = selectedDetails.Where(x => (x.FineAmount ?? 0) > 0).ToList();
                var totalOverdueFine = lateDetails.Sum(x => x.FineAmount ?? 0);

                var returnMessage = $"You have returned: {returnedBookTitles}. Thank you!";
                if (totalOverdueFine > 0)
                {
                    returnMessage += $" Overdue fine incurred: {totalOverdueFine:N0} VND.";
                }

                await notificationService.CreateAsync(new NotificationRequest
                {
                    UserId = transaction.UserId,
                    Title = "Books Returned",
                    Message = returnMessage,
                    Type = "Return"
                });
            }
            catch (Exception ex)
            {
                await auditLogService.LogAsync("ReturnNotificationError", "BorrowTransaction", transaction.BorrowTransactionId.ToString(), $"Failed to send return notification: {ex.Message}");
            }

            return Ok("Books returned successfully.", transaction.BorrowTransactionId);
        }

        public async Task<CirculationActionResponse> RenewAsync(RenewRequest request)
        {
            var detail = await circulationRepository.GetBorrowDetailAsync(request.BorrowDetailId);
            if (detail == null)
            {
                return Fail("Borrow detail not found.");
            }

            var validationError = await ValidateRenewalEligibilityAsync(detail);
            if (validationError != null)
            {
                return Fail(validationError);
            }

            var policy = await systemSettingService.GetPolicyAsync();
            var now = DateTime.UtcNow;
            var baseDate = detail.DueDate.Date < now.Date ? now.Date : detail.DueDate.Date;
            detail.DueDate = baseDate.AddDays(request.ExtraDays <= 0 ? policy.RenewDays : request.ExtraDays);
            detail.UpdatedAt = now;

            var transaction = detail.BorrowTransaction;
            transaction.DueDate = transaction.BorrowDetails
                .Where(x => x.ActualReturnDate == null)
                .Select(x => x.BorrowDetailId == detail.BorrowDetailId ? detail.DueDate : x.DueDate)
                .DefaultIfEmpty(detail.DueDate)
                .Max();
            transaction.Status = "Borrowing";
            transaction.UpdatedAt = now;

            await circulationRepository.SaveChangesAsync();
            await auditLogService.LogAsync("RenewBook", "BorrowDetail", detail.BorrowDetailId.ToString(), $"Borrow detail #{detail.BorrowDetailId} renewed until {detail.DueDate:yyyy-MM-dd}.");
            return Ok("Book renewed successfully.", transaction.BorrowTransactionId);
        }

        public async Task<string?> ValidateRenewalEligibilityAsync(BorrowDetail detail)
        {
            if (detail.ActualReturnDate != null)
            {
                return "Returned books cannot be renewed.";
            }

            var policy = await systemSettingService.GetPolicyAsync();
            var now = DateTime.UtcNow;

            if (detail.DueDate.Date < now.Date)
            {
                return "Overdue books cannot be renewed online. Please return the book or contact a librarian.";
            }

            var unpaidFines = await circulationRepository.GetTotalUnpaidFinesAsync(detail.BorrowTransaction.UserId);
            if (unpaidFines > 0)
            {
                return "You cannot renew books because you have unpaid fines. Please pay off your fines first.";
            }

            if (detail.BookCopy != null)
            {
                var pendingReservationsCount = await reservationService.CountPendingReservationsAsync(detail.BookCopy.BookId);
                if (pendingReservationsCount > 0)
                {
                    return "This book has a pending reservation waitlist. You cannot renew it.";
                }
            }

            var defaultDays = policy.DefaultLoanDays > 0 ? policy.DefaultLoanDays : 14;
            if ((detail.DueDate.Date - detail.BorrowDate.Date).Days > defaultDays)
            {
                return "This book copy has already been renewed once. You cannot renew it again.";
            }

            return null;
        }

        public async Task<CirculationActionResponse> ReportIssueAsync(ReportIssueRequest request)
        {
            if (request.Status != BookCopyStatus.Damaged && request.Status != BookCopyStatus.Lost)
            {
                return Fail("Only damaged or lost reports are supported here.");
            }

            var detail = await circulationRepository.GetBorrowDetailAsync(request.BorrowDetailId);
            if (detail == null)
            {
                return Fail("Borrow detail not found.");
            }

            if (detail.ActualReturnDate != null)
            {
                return Fail("This book was already returned.");
            }

            var policy = await systemSettingService.GetPolicyAsync();
            var now = DateTime.UtcNow;
            var configuredFine = request.Status == BookCopyStatus.Damaged ? policy.DamagedFine : policy.LostFine;
            detail.ActualReturnDate = now;
            detail.FineAmount = Math.Max(detail.FineAmount ?? 0, Math.Max(request.FineAmount, configuredFine));
            detail.IsFinePaid = detail.FineAmount.GetValueOrDefault() == 0;
            detail.UpdatedAt = now;

            if (detail.BookCopy != null)
            {
                detail.BookCopy.Status = request.Status;
                detail.BookCopy.Condition = request.Status == BookCopyStatus.Damaged
                    ? BookCondition.Damaged
                    : detail.BookCopy.Condition;
                detail.BookCopy.UpdatedAt = now;
            }

            UpdateTransactionStatus(detail.BorrowTransaction, now);
            await circulationRepository.SaveChangesAsync();
            await auditLogService.LogAsync("ReportBookIssue", "BorrowDetail", detail.BorrowDetailId.ToString(), $"Book copy issue reported as {request.Status} with fine {detail.FineAmount:0}.");

            return Ok("Issue reported successfully.", detail.BorrowTransactionId);
        }

        private async Task RefreshOverdueTransactionsAsync()
        {
            var now = DateTime.UtcNow;
            var transactions = await circulationRepository.QueryTransactionsForStatusUpdate()
                .Where(x => x.Status != "Returned")
                .ToListAsync();

            var changed = false;
            foreach (var transaction in transactions)
            {
                var hasOpenDetails = transaction.BorrowDetails.Any(x => x.ActualReturnDate == null);
                var shouldBeReturned = !hasOpenDetails;
                var shouldBeOverdue = hasOpenDetails && transaction.BorrowDetails
                    .Any(x => x.ActualReturnDate == null && x.DueDate.Date < now.Date);

                var newStatus = shouldBeReturned ? "Returned" : shouldBeOverdue ? "Overdue" : "Borrowing";
                if (transaction.Status != newStatus)
                {
                    transaction.Status = newStatus;
                    transaction.UpdatedAt = now;
                    changed = true;
                }
            }

            if (changed)
            {
                await circulationRepository.SaveChangesAsync();
            }
        }

        private static void UpdateTransactionStatus(BorrowTransaction transaction, DateTime now)
        {
            var openDetails = transaction.BorrowDetails.Where(x => x.ActualReturnDate == null).ToList();
            if (!openDetails.Any())
            {
                transaction.Status = "Returned";
            }
            else
            {
                transaction.DueDate = openDetails.Max(x => x.DueDate);
                transaction.Status = openDetails.Any(x => x.DueDate.Date < now.Date) ? "Overdue" : "Borrowing";
            }

            transaction.UpdatedAt = now;
        }

        private static CirculationTransactionItem MapTransaction(BorrowTransaction transaction)
        {
            return new CirculationTransactionItem
            {
                BorrowTransactionId = transaction.BorrowTransactionId,
                UserId = transaction.UserId,
                UserFullName = transaction.Account?.FullName ?? string.Empty,
                BorrowDate = transaction.BorrowDate,
                DueDate = transaction.DueDate,
                Status = transaction.Status,
                BorrowDetails = transaction.BorrowDetails
                    .OrderBy(x => x.DueDate)
                    .Select(MapDetail)
                    .ToList()
            };
        }

        private static CirculationBorrowDetailItem MapDetail(BorrowDetail detail)
        {
            return new CirculationBorrowDetailItem
            {
                BorrowDetailId = detail.BorrowDetailId,
                BookCopyId = detail.BookCopyId,
                BookId = detail.BookCopy?.BookId,
                BookTitle = detail.BookCopy?.Book?.Title ?? string.Empty,
                ImageUrl = detail.BookCopy?.Book?.ImageUrl,
                Barcode = detail.BookCopy?.Barcode ?? string.Empty,
                AuthorName = detail.BookCopy?.Book?.Author?.Name ?? string.Empty,
                BorrowDate = detail.BorrowDate,
                DueDate = detail.DueDate,
                ActualReturnDate = detail.ActualReturnDate,
                FineAmount = detail.FineAmount
            };
        }

        private static CirculationActionResponse Ok(string message, int transactionId)
        {
            return new CirculationActionResponse
            {
                IsSuccess = true,
                Message = message,
                TransactionId = transactionId
            };
        }

        private static CirculationActionResponse Fail(string message)
        {
            return new CirculationActionResponse
            {
                IsSuccess = false,
                Message = message
            };
        }
    }
}
