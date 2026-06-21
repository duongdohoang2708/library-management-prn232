using LibraryManagement.BLL.DTO.Notification;
using LibraryManagement.DAL.Repositories;
using LibraryManagementDAL.Models;

namespace LibraryManagement.BLL.Services
{
    public class DueReminderService
    {
        private readonly ReminderRepository reminderRepository;
        private readonly NotificationService notificationService;
        private readonly SystemSettingService systemSettingService;
        private readonly AuditLogService auditLogService;

        public DueReminderService(
            ReminderRepository _reminderRepository,
            NotificationService _notificationService,
            SystemSettingService _systemSettingService,
            AuditLogService _auditLogService)
        {
            reminderRepository = _reminderRepository;
            notificationService = _notificationService;
            systemSettingService = _systemSettingService;
            auditLogService = _auditLogService;
        }

        public async Task<int> SendDueRemindersAsync()
        {
            var policy = await systemSettingService.GetPolicyAsync();
            var today = DateTime.UtcNow.Date;
            var details = await reminderRepository.GetOpenBorrowDetailsAsync();
            var sentCount = 0;

            foreach (var detail in details)
            {
                var dueDate = detail.DueDate.Date;
                var daysToDue = (dueDate - today).Days;
                var isDueSoon = policy.DueSoonReminderDays > 0 && daysToDue >= 0 && daysToDue <= policy.DueSoonReminderDays;
                var isOverdue = dueDate < today;

                if (!isDueSoon && !isOverdue)
                {
                    continue;
                }

                var reminderType = isOverdue ? "Overdue" : "DueSoon";
                if (await reminderRepository.HasReminderAsync(detail.BorrowDetailId, reminderType, today))
                {
                    continue;
                }

                var bookTitle = detail.BookCopy?.Book?.Title ?? "your borrowed book";
                var message = isOverdue
                    ? $"Your borrowed book \"{bookTitle}\" is overdue. A fine of {policy.OverdueFinePerDay:0} VND per day may apply."
                    : $"Your borrowed book \"{bookTitle}\" is due on {detail.DueDate:dd/MM/yyyy}. Please return or renew it on time.";

                await notificationService.CreateAsync(new NotificationRequest
                {
                    UserId = detail.BorrowTransaction.UserId,
                    Title = isOverdue ? "Borrowed book is overdue" : "Borrowed book is due soon",
                    Message = message,
                    Type = reminderType
                });

                reminderRepository.AddReminder(new ReminderLog
                {
                    BorrowDetailId = detail.BorrowDetailId,
                    ReminderType = reminderType,
                    ReminderDate = today,
                    CreatedAt = DateTime.UtcNow
                });
                sentCount++;
            }

            await reminderRepository.SaveChangesAsync();
            if (sentCount > 0)
            {
                await auditLogService.LogAsync("SendDueReminders", "BorrowDetail", null, $"{sentCount} due/overdue reminders sent.");
            }

            return sentCount;
        }
    }
}
