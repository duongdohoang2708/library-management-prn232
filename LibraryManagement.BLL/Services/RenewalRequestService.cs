using LibraryManagement.BLL.DTO.Notification;
using LibraryManagement.DAL.Repositories;
using LibraryManagementDAL.DTO.RenewalRequest;
using LibraryManagementDAL.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.BLL.Services
{
    public class RenewalRequestService
    {
        private readonly RenewalRequestRepository renewalRequestRepository;
        private readonly CirculationRepository circulationRepository;
        private readonly CirculationService circulationService;
        private readonly SystemSettingService systemSettingService;
        private readonly NotificationService notificationService;
        private readonly AuditLogService auditLogService;

        public RenewalRequestService(
            RenewalRequestRepository _renewalRequestRepository,
            CirculationRepository _circulationRepository,
            CirculationService _circulationService,
            SystemSettingService _systemSettingService,
            NotificationService _notificationService,
            AuditLogService _auditLogService)
        {
            renewalRequestRepository = _renewalRequestRepository;
            circulationRepository = _circulationRepository;
            circulationService = _circulationService;
            systemSettingService = _systemSettingService;
            notificationService = _notificationService;
            auditLogService = _auditLogService;
        }

        public async Task<List<RenewalRequestItem>> GetPendingForStaffAsync()
        {
            var requests = await renewalRequestRepository.QueryRequests()
                .Where(x => x.Status == RenewalRequestStatus.Pending)
                .OrderBy(x => x.RequestedAt)
                .ToListAsync();

            return requests.Select(MapItem).ToList();
        }

        public async Task<List<RenewalRequestItem>> GetByUserAsync(int userId)
        {
            var requests = await renewalRequestRepository.QueryRequests()
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.RequestedAt)
                .ToListAsync();

            return requests.Select(MapItem).ToList();
        }

        public async Task<RenewalRequestActionResponse> CreateRequestAsync(RenewalRequestCreateRequest request)
        {
            var detail = await circulationRepository.GetBorrowDetailAsync(request.BorrowDetailId);
            if (detail == null)
            {
                return Fail("Borrow detail not found.");
            }

            if (detail.BorrowTransaction.UserId != request.UserId)
            {
                return Fail("You can only request renewal for your own borrowed books.");
            }

            var validationError = await circulationService.ValidateRenewalEligibilityAsync(detail);
            if (validationError != null)
            {
                return Fail(validationError);
            }

            if (await renewalRequestRepository.HasPendingRequestAsync(request.BorrowDetailId))
            {
                return Fail("You already have a pending renewal request for this book.");
            }

            var policy = await systemSettingService.GetPolicyAsync();
            var now = DateTime.UtcNow;
            var extraDays = request.ExtraDays <= 0 ? policy.RenewDays : request.ExtraDays;

            var renewalRequest = new RenewalRequest
            {
                UserId = request.UserId,
                BorrowDetailId = request.BorrowDetailId,
                RequestedExtraDays = extraDays,
                RequestedAt = now,
                Status = RenewalRequestStatus.Pending,
                CreatedAt = now
            };

            renewalRequestRepository.Add(renewalRequest);
            await renewalRequestRepository.SaveChangesAsync();

            var bookTitle = detail.BookCopy?.Book?.Title ?? "your book";
            var account = await circulationRepository.GetAccountAsync(request.UserId);
            var memberName = account?.FullName ?? "A member";

            await NotifyMemberAsync(
                request.UserId,
                "Renewal request submitted",
                $"Your renewal request for \"{bookTitle}\" has been submitted and is awaiting librarian approval.");

            await NotifyStaffAsync(
                "New renewal request",
                $"{memberName} requested to renew \"{bookTitle}\" (due {detail.DueDate:dd/MM/yyyy}).");

            await auditLogService.LogAsync(
                "CreateRenewalRequest",
                "RenewalRequest",
                renewalRequest.RenewalRequestId.ToString(),
                $"User #{request.UserId} requested renewal for borrow detail #{request.BorrowDetailId}.");

            return new RenewalRequestActionResponse
            {
                IsSuccess = true,
                Message = "Your renewal request has been submitted. You will be notified once a librarian reviews it.",
                RenewalRequestId = renewalRequest.RenewalRequestId
            };
        }

        public async Task<RenewalRequestActionResponse> ApproveAsync(int renewalRequestId, int reviewerUserId)
        {
            var request = await renewalRequestRepository.GetRequestAsync(renewalRequestId);
            if (request == null)
            {
                return Fail("Renewal request not found.");
            }

            if (request.Status != RenewalRequestStatus.Pending)
            {
                return Fail("Only pending renewal requests can be approved.");
            }

            var renewResult = await circulationService.RenewAsync(new LibraryManagementDAL.DTO.Circulation.RenewRequest
            {
                BorrowDetailId = request.BorrowDetailId,
                ExtraDays = request.RequestedExtraDays
            });

            if (!renewResult.IsSuccess)
            {
                return Fail(renewResult.Message);
            }

            var detail = await circulationRepository.GetBorrowDetailAsync(request.BorrowDetailId);
            var now = DateTime.UtcNow;

            request.Status = RenewalRequestStatus.Approved;
            request.ReviewedByUserId = reviewerUserId;
            request.ReviewedAt = now;
            request.NewDueDate = detail?.DueDate;
            request.UpdatedAt = now;

            await renewalRequestRepository.SaveChangesAsync();

            var bookTitle = detail?.BookCopy?.Book?.Title ?? "your book";
            await NotifyMemberAsync(
                request.UserId,
                "Renewal request approved",
                $"Your renewal request for \"{bookTitle}\" has been approved. New due date: {request.NewDueDate:dd/MM/yyyy}.");

            await auditLogService.LogAsync(
                "ApproveRenewalRequest",
                "RenewalRequest",
                renewalRequestId.ToString(),
                $"Reviewer #{reviewerUserId} approved renewal request #{renewalRequestId}.");

            return new RenewalRequestActionResponse
            {
                IsSuccess = true,
                Message = "Renewal request approved and due date updated.",
                RenewalRequestId = renewalRequestId
            };
        }

        public async Task<RenewalRequestActionResponse> RejectAsync(int renewalRequestId, int reviewerUserId, string reason)
        {
            reason = reason?.Trim() ?? string.Empty;
            if (reason.Length < 5)
            {
                return Fail("Rejection reason must be at least 5 characters.");
            }

            var request = await renewalRequestRepository.GetRequestAsync(renewalRequestId);
            if (request == null)
            {
                return Fail("Renewal request not found.");
            }

            if (request.Status != RenewalRequestStatus.Pending)
            {
                return Fail("Only pending renewal requests can be rejected.");
            }

            var now = DateTime.UtcNow;
            request.Status = RenewalRequestStatus.Rejected;
            request.ReviewedByUserId = reviewerUserId;
            request.ReviewedAt = now;
            request.RejectionReason = reason;
            request.UpdatedAt = now;

            await renewalRequestRepository.SaveChangesAsync();

            var bookTitle = request.BorrowDetail?.BookCopy?.Book?.Title ?? "your book";
            await NotifyMemberAsync(
                request.UserId,
                "Renewal request rejected",
                $"Your renewal request for \"{bookTitle}\" was rejected. Reason: {reason}");

            await auditLogService.LogAsync(
                "RejectRenewalRequest",
                "RenewalRequest",
                renewalRequestId.ToString(),
                $"Reviewer #{reviewerUserId} rejected renewal request #{renewalRequestId}. Reason: {reason}");

            return new RenewalRequestActionResponse
            {
                IsSuccess = true,
                Message = "Renewal request rejected.",
                RenewalRequestId = renewalRequestId
            };
        }

        public async Task<RenewalRequestActionResponse> CancelAsync(int renewalRequestId, int userId)
        {
            var request = await renewalRequestRepository.GetRequestAsync(renewalRequestId);
            if (request == null)
            {
                return Fail("Renewal request not found.");
            }

            if (request.UserId != userId)
            {
                return Fail("You can only cancel your own renewal requests.");
            }

            if (request.Status != RenewalRequestStatus.Pending)
            {
                return Fail("Only pending renewal requests can be cancelled.");
            }

            request.Status = RenewalRequestStatus.Cancelled;
            request.UpdatedAt = DateTime.UtcNow;

            await renewalRequestRepository.SaveChangesAsync();

            await auditLogService.LogAsync(
                "CancelRenewalRequest",
                "RenewalRequest",
                renewalRequestId.ToString(),
                $"User #{userId} cancelled renewal request #{renewalRequestId}.");

            return new RenewalRequestActionResponse
            {
                IsSuccess = true,
                Message = "Renewal request cancelled.",
                RenewalRequestId = renewalRequestId
            };
        }

        private async Task NotifyMemberAsync(int userId, string title, string message)
        {
            try
            {
                await notificationService.CreateAsync(new NotificationRequest
                {
                    UserId = userId,
                    Title = title,
                    Message = message,
                    Type = "RenewalRequest"
                });
            }
            catch
            {
                // Notification failure should not block the main flow.
            }
        }

        private async Task NotifyStaffAsync(string title, string message)
        {
            var staffUserIds = await renewalRequestRepository.GetStaffUserIdsAsync();
            foreach (var staffUserId in staffUserIds)
            {
                try
                {
                    await notificationService.CreateAsync(new NotificationRequest
                    {
                        UserId = staffUserId,
                        Title = title,
                        Message = message,
                        Type = "RenewalRequest"
                    });
                }
                catch
                {
                    // Continue notifying other staff members.
                }
            }
        }

        private static RenewalRequestItem MapItem(RenewalRequest request)
        {
            return new RenewalRequestItem
            {
                RenewalRequestId = request.RenewalRequestId,
                UserId = request.UserId,
                UserFullName = request.Account?.FullName ?? string.Empty,
                UserEmail = request.Account?.Email ?? string.Empty,
                BorrowDetailId = request.BorrowDetailId,
                BookTitle = request.BorrowDetail?.BookCopy?.Book?.Title ?? string.Empty,
                Barcode = request.BorrowDetail?.BookCopy?.Barcode ?? string.Empty,
                CurrentDueDate = request.BorrowDetail?.DueDate ?? default,
                RequestedExtraDays = request.RequestedExtraDays,
                RequestedAt = request.RequestedAt,
                Status = request.Status,
                NewDueDate = request.NewDueDate,
                RejectionReason = request.RejectionReason,
                ReviewedAt = request.ReviewedAt,
                ReviewedByName = request.ReviewedBy?.FullName
            };
        }

        private static RenewalRequestActionResponse Fail(string message)
        {
            return new RenewalRequestActionResponse
            {
                IsSuccess = false,
                Message = message
            };
        }
    }
}
