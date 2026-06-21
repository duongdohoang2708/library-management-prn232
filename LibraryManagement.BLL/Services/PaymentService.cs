using LibraryManagementDAL.DTO.Payment;
using LibraryManagementDAL.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LibraryManagement.DAL.Data;

namespace LibraryManagement.BLL.Services
{
    public class PaymentService
    {
        private readonly ApplicationDbContext _db;

        public PaymentService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<List<UserFinesDTO>> GetUsersWithUnpaidFinesAsync()
        {
            var usersWithFines = await _db.BorrowDetails
                .Include(bd => bd.BorrowTransaction)
                    .ThenInclude(bt => bt.Account) // Changed User to Account
                .Where(bd => bd.FineAmount > (bd.FinePaidAmount ?? 0))
                .GroupBy(bd => new { bd.BorrowTransaction.UserId, bd.BorrowTransaction.Account.FullName, bd.BorrowTransaction.Account.Email })
                .Select(g => new UserFinesDTO
                {
                    UserId = g.Key.UserId,
                    FullName = g.Key.FullName ?? "Unknown",
                    Email = g.Key.Email ?? "Unknown",
                    TotalUnpaidFine = g.Sum(bd => bd.FineAmount ?? 0) - g.Sum(bd => bd.FinePaidAmount ?? 0),
                    UnpaidBookCount = g.Count()
                })
                .ToListAsync();

            return usersWithFines;
        }

        public async Task<(List<UserFinesDTO> Users, int TotalCount)> GetPagedUsersWithUnpaidFinesAsync(int page, int pageSize, string? searchQuery = null)
        {
            var query = _db.BorrowDetails
                .Include(bd => bd.BorrowTransaction)
                    .ThenInclude(bt => bt.Account)
                .Where(bd => bd.FineAmount > (bd.FinePaidAmount ?? 0))
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchQuery))
            {
                var lowerSearch = searchQuery.ToLower();
                query = query.Where(bd => 
                    bd.BorrowTransaction.UserId.ToString().Contains(lowerSearch) ||
                    (bd.BorrowTransaction.Account.FullName != null && bd.BorrowTransaction.Account.FullName.ToLower().Contains(lowerSearch)) ||
                    (bd.BorrowTransaction.Account.Email != null && bd.BorrowTransaction.Account.Email.ToLower().Contains(lowerSearch))
                );
            }

            var groupedQuery = query
                .GroupBy(bd => new { bd.BorrowTransaction.UserId, bd.BorrowTransaction.Account.FullName, bd.BorrowTransaction.Account.Email })
                .Select(g => new UserFinesDTO
                {
                    UserId = g.Key.UserId,
                    FullName = g.Key.FullName ?? "Unknown",
                    Email = g.Key.Email ?? "Unknown",
                    TotalUnpaidFine = g.Sum(bd => bd.FineAmount ?? 0) - g.Sum(bd => bd.FinePaidAmount ?? 0),
                    UnpaidBookCount = g.Count()
                });

            int totalCount = await groupedQuery.CountAsync();
            
            var users = await groupedQuery
                .OrderBy(u => u.FullName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (users, totalCount);
        }

        public async Task<List<BorrowDetail>> GetUnpaidFinesByUserIdAsync(int userId)
        {
            return await _db.BorrowDetails
                .Include(bd => bd.BookCopy)
                    .ThenInclude(bc => bc.Book)
                .Include(bd => bd.BorrowTransaction)
                .Where(bd => bd.BorrowTransaction.UserId == userId && bd.FineAmount > (bd.FinePaidAmount ?? 0))
                .OrderBy(bd => bd.ActualReturnDate)
                .ToListAsync();
        }

        public async Task<Payment> ProcessPaymentAsync(int userId, List<int> borrowDetailIds, decimal amountPaid, PaymentMethod method)
        {
            if (borrowDetailIds == null || !borrowDetailIds.Any())
                throw new Exception("Không có khoản phạt nào được chọn để thanh toán.");

            if (amountPaid <= 0)
                throw new Exception("Số tiền thanh toán phải lớn hơn 0.");

            var details = await _db.BorrowDetails
                .Include(bd => bd.BorrowTransaction)
                .Where(bd => borrowDetailIds.Contains(bd.BorrowDetailId) && bd.BorrowTransaction.UserId == userId)
                .ToListAsync();

            if (details.Count != borrowDetailIds.Count)
                throw new Exception("Một hoặc nhiều khoản phạt không hợp lệ hoặc không thuộc về người dùng này.");

            decimal totalOwed = details.Sum(bd => (bd.FineAmount ?? 0) - (bd.FinePaidAmount ?? 0));
            if (amountPaid > totalOwed)
                throw new Exception($"Số tiền thanh toán ({amountPaid}) vượt quá tổng số tiền nợ ({totalOwed}).");

            var payment = new Payment
            {
                UserId = userId,
                Amount = amountPaid,
                PaidAt = DateTime.UtcNow,
                PaymentMethod = method,
                PaymentStatus = PaymentStatus.Success,
                TransactionCode = $"PAY-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString().Substring(0, 4).ToUpper()}",
                CreatedAt = DateTime.UtcNow,
                PaymentDetails = new List<PaymentDetail>()
            };

            decimal remainingAmount = amountPaid;
            foreach (var detail in details)
            {
                if (remainingAmount <= 0) break;

                decimal owedOnThisDetail = (detail.FineAmount ?? 0) - (detail.FinePaidAmount ?? 0);
                if (owedOnThisDetail <= 0) continue;

                decimal amountToApply = Math.Min(remainingAmount, owedOnThisDetail);
                
                payment.PaymentDetails.Add(new PaymentDetail
                {
                    BorrowDetailId = detail.BorrowDetailId,
                    Amount = amountToApply
                });

                detail.FinePaidAmount = (detail.FinePaidAmount ?? 0) + amountToApply;
                detail.IsFinePaid = detail.FinePaidAmount >= detail.FineAmount;
                
                remainingAmount -= amountToApply;
            }

            _db.Payments.Add(payment);
            await _db.SaveChangesAsync();

            return payment;
        }

        public async Task<List<BorrowDetail>> GetMyFinesAsync(int userId)
        {
            return await _db.BorrowDetails
                .Include(bd => bd.BookCopy)
                    .ThenInclude(bc => bc.Book)
                .Include(bd => bd.BorrowTransaction)
                .Where(bd => bd.BorrowTransaction.UserId == userId && bd.FineAmount > (bd.FinePaidAmount ?? 0))
                .OrderByDescending(bd => bd.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Payment>> GetMyPaymentHistoryAsync(int userId)
        {
            return await _db.Payments
                .Include(p => p.PaymentDetails)
                    .ThenInclude(pd => pd.BorrowDetail)
                        .ThenInclude(bd => bd.BookCopy)
                            .ThenInclude(bc => bc.Book)
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }
        public async Task<BorrowDetail> CreateManualFineAsync(int userId, decimal amount, string reason)
        {
            var user = await _db.Accounts.FindAsync(userId);
            if (user == null) throw new Exception("Không tìm thấy người dùng.");

            // Create a "Manual Fine" transaction
            var trans = new BorrowTransaction
            {
                UserId = userId,
                BorrowDate = DateTime.UtcNow,
                DueDate = DateTime.UtcNow,
                Status = "ManualFine",
                CreatedAt = DateTime.UtcNow,
                BorrowDetails = new List<BorrowDetail>()
            };

            var detail = new BorrowDetail
            {
                BorrowTransaction = trans,
                BookCopyId = null, // Floating fine
                BorrowDate = DateTime.UtcNow,
                DueDate = DateTime.UtcNow,
                FineAmount = amount,
                FinePaidAmount = 0,
                IsFinePaid = false,
                CreatedAt = DateTime.UtcNow
                // We could add the 'reason' to a Note field if we had one, 
                // but for now, the amount being tied to a 'ManualFine' status transaction is the key.
            };

            _db.BorrowTransactions.Add(trans);
            _db.BorrowDetails.Add(detail);
            await _db.SaveChangesAsync();

            return detail;
        }
    }
}
