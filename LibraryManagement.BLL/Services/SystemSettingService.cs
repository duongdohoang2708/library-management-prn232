using LibraryManagement.BLL.DTO.Common;
using LibraryManagement.BLL.DTO.Settings;
using LibraryManagement.DAL.Repositories;
using LibraryManagementDAL.Models;

namespace LibraryManagement.BLL.Services
{
    public class SystemSettingService
    {
        private readonly SystemSettingRepository systemSettingRepository;
        private readonly AuditLogService auditLogService;

        public SystemSettingService(SystemSettingRepository _systemSettingRepository, AuditLogService _auditLogService)
        {
            systemSettingRepository = _systemSettingRepository;
            auditLogService = _auditLogService;
        }

        public async Task<LibraryPolicySettings> GetPolicyAsync()
        {
            await EnsureDefaultsAsync();
            var settings = await systemSettingRepository.GetAllAsync();

            return new LibraryPolicySettings
            {
                DefaultLoanDays = GetInt(settings, SettingKeys.DefaultLoanDays, 14),
                RenewDays = GetInt(settings, SettingKeys.RenewDays, 7),
                ReservationHoldDays = GetInt(settings, SettingKeys.ReservationHoldDays, 3),
                MaxOpenBorrowedBooks = GetInt(settings, SettingKeys.MaxOpenBorrowedBooks, 5),
                DueSoonReminderDays = GetInt(settings, SettingKeys.DueSoonReminderDays, 1),
                OverdueFinePerDay = GetDecimal(settings, SettingKeys.OverdueFinePerDay, 5000m),
                DamagedFine = GetDecimal(settings, SettingKeys.DamagedFine, 50000m),
                LostFine = GetDecimal(settings, SettingKeys.LostFine, 100000m)
            };
        }

        public async Task<ActionResponse> UpdatePolicyAsync(LibraryPolicySettings request)
        {
            if (request.DefaultLoanDays < 1 ||
                request.RenewDays < 1 ||
                request.ReservationHoldDays < 1 ||
                request.MaxOpenBorrowedBooks < 1 ||
                request.DueSoonReminderDays < 0 ||
                request.OverdueFinePerDay < 0 ||
                request.DamagedFine < 0 ||
                request.LostFine < 0)
            {
                return new ActionResponse { IsSuccess = false, Message = "Policy values must be valid positive numbers." };
            }

            await EnsureDefaultsAsync();
            await SetAsync(SettingKeys.DefaultLoanDays, request.DefaultLoanDays.ToString(), "Default loan period in days.");
            await SetAsync(SettingKeys.RenewDays, request.RenewDays.ToString(), "Extra days added when renewing a book.");
            await SetAsync(SettingKeys.ReservationHoldDays, request.ReservationHoldDays.ToString(), "Days an allocated reservation is held.");
            await SetAsync(SettingKeys.MaxOpenBorrowedBooks, request.MaxOpenBorrowedBooks.ToString(), "Maximum open borrowed books per member.");
            await SetAsync(SettingKeys.DueSoonReminderDays, request.DueSoonReminderDays.ToString(), "Send due-soon reminders this many days before due date.");
            await SetAsync(SettingKeys.OverdueFinePerDay, request.OverdueFinePerDay.ToString("0.##"), "Overdue fine per day.");
            await SetAsync(SettingKeys.DamagedFine, request.DamagedFine.ToString("0.##"), "Fine for damaged book reports.");
            await SetAsync(SettingKeys.LostFine, request.LostFine.ToString("0.##"), "Fine for lost book reports.");
            await systemSettingRepository.SaveChangesAsync();

            await auditLogService.LogAsync("UpdatePolicy", "SystemSettings", null, "Library policy settings updated.");
            return new ActionResponse { IsSuccess = true, Message = "System policy updated successfully." };
        }

        private async Task EnsureDefaultsAsync()
        {
            var defaults = new Dictionary<string, (string Value, string Description)>
            {
                [SettingKeys.DefaultLoanDays] = ("14", "Default loan period in days."),
                [SettingKeys.RenewDays] = ("7", "Extra days added when renewing a book."),
                [SettingKeys.ReservationHoldDays] = ("3", "Days an allocated reservation is held."),
                [SettingKeys.MaxOpenBorrowedBooks] = ("5", "Maximum open borrowed books per member."),
                [SettingKeys.DueSoonReminderDays] = ("1", "Send due-soon reminders this many days before due date."),
                [SettingKeys.OverdueFinePerDay] = ("5000", "Overdue fine per day."),
                [SettingKeys.DamagedFine] = ("50000", "Fine for damaged book reports."),
                [SettingKeys.LostFine] = ("100000", "Fine for lost book reports.")
            };

            var changed = false;
            foreach (var item in defaults)
            {
                if (await systemSettingRepository.GetByKeyAsync(item.Key) != null)
                {
                    continue;
                }

                systemSettingRepository.Add(new SystemSetting
                {
                    Key = item.Key,
                    Value = item.Value.Value,
                    Description = item.Value.Description,
                    CreatedAt = DateTime.UtcNow
                });
                changed = true;
            }

            if (changed)
            {
                await systemSettingRepository.SaveChangesAsync();
            }
        }

        private async Task SetAsync(string key, string value, string description)
        {
            var setting = await systemSettingRepository.GetByKeyAsync(key);
            if (setting == null)
            {
                systemSettingRepository.Add(new SystemSetting
                {
                    Key = key,
                    Value = value,
                    Description = description,
                    CreatedAt = DateTime.UtcNow
                });
                return;
            }

            setting.Value = value;
            setting.Description = description;
            setting.UpdatedAt = DateTime.UtcNow;
        }

        private static int GetInt(List<SystemSetting> settings, string key, int fallback)
        {
            var value = settings.FirstOrDefault(x => x.Key == key)?.Value;
            return int.TryParse(value, out var parsed) ? parsed : fallback;
        }

        private static decimal GetDecimal(List<SystemSetting> settings, string key, decimal fallback)
        {
            var value = settings.FirstOrDefault(x => x.Key == key)?.Value;
            return decimal.TryParse(value, out var parsed) ? parsed : fallback;
        }

        private static class SettingKeys
        {
            public const string DefaultLoanDays = "Borrow.DefaultLoanDays";
            public const string RenewDays = "Borrow.RenewDays";
            public const string ReservationHoldDays = "Reservation.HoldDays";
            public const string MaxOpenBorrowedBooks = "Borrow.MaxOpenBorrowedBooks";
            public const string DueSoonReminderDays = "Reminder.DueSoonDays";
            public const string OverdueFinePerDay = "Fine.OverduePerDay";
            public const string DamagedFine = "Fine.Damaged";
            public const string LostFine = "Fine.Lost";
        }
    }
}
