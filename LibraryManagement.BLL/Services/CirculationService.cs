using LibraryManagement.DAL.Repositories;
using LibraryManagementDAL.DTO.Circulation;
using LibraryManagementDAL.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.BLL.Services
{
    public class CirculationService
    {
        private const int PageSize = 10;
        private readonly CirculationRepository circulationRepository;
        private readonly ReservationService reservationService;
        private readonly SystemSettingService systemSettingService;
        private readonly AuditLogService auditLogService;

        public CirculationService(
            CirculationRepository _circulationRepository,
            ReservationService _reservationService,
            SystemSettingService _systemSettingService,
            AuditLogService _auditLogService)
        {
            circulationRepository = _circulationRepository;
            reservationService = _reservationService;
            systemSettingService = _systemSettingService;
            auditLogService = _auditLogService;
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
                .OrderByDescending(x => x.Status != "Returned")
                .ThenBy(x => x.DueDate)
                .ThenByDescending(x => x.BorrowDate);

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

            if (barcodes.Count > 5)
            {
                return Fail("A transaction can borrow at most 5 books.");
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

            return new CirculationActionResponse
            {
                IsSuccess = true,
                Message = "Borrow transaction created successfully.",
                TransactionId = transaction.BorrowTransactionId
            };
        }

        public async Task<CirculationActionResponse> MemberBorrowNowAsync(MemberBorrowRequest request)
        {
            var user = await circulationRepository.GetMemberAccountAsync(request.UserId);
            if (user == null)
            {
                return Fail("Please login before borrowing.");
            }

            if (user.Member == null)
            {
                return Fail("Only member accounts can use Borrow Now.");
            }

            var policy = await systemSettingService.GetPolicyAsync();
            var openBorrowedBooks = await circulationRepository.CountOpenBorrowedBooksAsync(user.UserId);
            if (openBorrowedBooks >= policy.MaxOpenBorrowedBooks)
            {
                return Fail($"You can borrow at most {policy.MaxOpenBorrowedBooks} books at the same time.");
            }

            var copy = await circulationRepository.GetFirstAvailableCopyByBookIdAsync(request.BookId);
            if (copy == null)
            {
                return Fail("No available copy for this book. Please use Reserve Queue.");
            }

            var now = DateTime.UtcNow;
            var dueDate = now.Date.AddDays(request.LoanDays <= 0 ? policy.DefaultLoanDays : request.LoanDays);

            copy.Status = BookCopyStatus.Borrowed;
            copy.UpdatedAt = now;

            var transaction = new BorrowTransaction
            {
                UserId = user.UserId,
                BorrowDate = now,
                DueDate = dueDate,
                Status = "Borrowing",
                CreatedAt = now,
                BorrowDetails = new List<BorrowDetail>
                {
                    new BorrowDetail
                    {
                        BookCopyId = copy.BookCopyId,
                        BorrowDate = now,
                        DueDate = dueDate,
                        FineAmount = 0,
                        FinePaidAmount = 0,
                        IsFinePaid = true,
                        CreatedAt = now
                    }
                }
            };

            circulationRepository.AddTransaction(transaction);
            await circulationRepository.SaveChangesAsync();
            await auditLogService.LogAsync("MemberBorrowNow", "BorrowTransaction", transaction.BorrowTransactionId.ToString(), $"Member #{user.UserId} borrowed book #{request.BookId}.");

            return Ok("Book borrowed successfully. Check your Borrowed Books list.", transaction.BorrowTransactionId);
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

            return Ok("Books returned successfully.", transaction.BorrowTransactionId);
        }

        public async Task<CirculationActionResponse> RenewAsync(RenewRequest request)
        {
            var detail = await circulationRepository.GetBorrowDetailAsync(request.BorrowDetailId);
            if (detail == null)
            {
                return Fail("Borrow detail not found.");
            }

            if (detail.ActualReturnDate != null)
            {
                return Fail("Returned books cannot be renewed.");
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
