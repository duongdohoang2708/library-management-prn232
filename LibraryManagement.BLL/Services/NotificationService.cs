using LibraryManagement.BLL.DTO.Common;
using LibraryManagement.BLL.DTO.Notification;
using LibraryManagement.BLL.DTO.User;
using LibraryManagement.DAL.Repositories;
using LibraryManagementDAL.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MimeKit;

namespace LibraryManagement.BLL.Services
{
    public class NotificationService
    {
        private const int PageSize = 10;
        private readonly NotificationRepository notificationRepository;
        private readonly IConfiguration configuration;

        public NotificationService(NotificationRepository _notificationRepository, IConfiguration _configuration)
        {
            notificationRepository = _notificationRepository;
            configuration = _configuration;
        }

        public async Task<PaginatedResult<Notification>> GetUserNotificationsAsync(int userId, int page)
        {
            page = page < 1 ? 1 : page;
            var query = notificationRepository.QueryNotifications()
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.CreatedAt);

            var totalCount = await query.CountAsync();
            var totalPages = Math.Max(1, (int)Math.Ceiling(totalCount / (double)PageSize));
            page = Math.Min(page, totalPages);

            return new PaginatedResult<Notification>
            {
                Items = await query.Skip((page - 1) * PageSize).Take(PageSize).ToListAsync(),
                PageNumber = page,
                CurrentPage = page,
                PageSize = PageSize,
                TotalCount = totalCount,
                TotalPages = totalPages,
                HasPreviousPage = page > 1,
                HasNextPage = page < totalPages
            };
        }

        public async Task<ActionResponse> CreateAsync(NotificationRequest request, bool sendEmail = true)
        {
            var account = await notificationRepository.GetAccountAsync(request.UserId);
            if (account == null)
            {
                return Fail("User not found.");
            }

            if (string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.Message))
            {
                return Fail("Title and message are required.");
            }

            var now = DateTime.UtcNow;
            var notification = new Notification
            {
                UserId = account.UserId,
                Title = request.Title.Trim(),
                Message = request.Message.Trim(),
                Type = string.IsNullOrWhiteSpace(request.Type) ? "General" : request.Type.Trim(),
                IsRead = false,
                IsSent = false,
                CreatedAt = now,
                ScheduledAt = now
            };

            notificationRepository.Add(notification);

            if (sendEmail)
            {
                try
                {
                    await SendEmailAsync(account.Email, account.FullName, notification.Title, notification.Message);
                    notification.IsSent = true;
                    notification.SentAt = now;
                }
                catch
                {
                    notification.IsSent = false;
                }
            }

            await notificationRepository.SaveChangesAsync();

            return new ActionResponse
            {
                IsSuccess = true,
                Message = notification.IsSent
                    ? "Notification sent successfully."
                    : "Notification saved. Email could not be sent.",
                Id = notification.NotificationId
            };
        }

        public async Task MarkAllAsReadAsync(int userId)
        {
            await notificationRepository.MarkAllAsReadAsync(userId);
            await notificationRepository.SaveChangesAsync();
        }

        private async Task SendEmailAsync(string email, string fullName, string title, string message)
        {
            var smtpServer = configuration["EmailSettings:SmtpServer"];
            var smtpPortText = configuration["EmailSettings:SmtpPort"];
            var senderEmail = configuration["EmailSettings:SenderEmail"];
            var senderPassword = configuration["EmailSettings:SenderPassword"];
            var senderName = configuration["EmailSettings:DisplaySenderName"] ?? "LMS System";

            if (string.IsNullOrWhiteSpace(smtpServer) ||
                string.IsNullOrWhiteSpace(smtpPortText) ||
                string.IsNullOrWhiteSpace(senderEmail) ||
                string.IsNullOrWhiteSpace(senderPassword) ||
                !int.TryParse(smtpPortText, out var smtpPort))
            {
                throw new InvalidOperationException("EmailSettings SMTP is not configured.");
            }

            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress(senderName, senderEmail));
            emailMessage.To.Add(new MailboxAddress(fullName, email));
            emailMessage.Subject = title;
            emailMessage.Body = new TextPart("plain")
            {
                Text = $"Hello {fullName},\n\n{message}\n\nLMS Standard"
            };

            using var smtpClient = new SmtpClient();
            await smtpClient.ConnectAsync(smtpServer, smtpPort, SecureSocketOptions.StartTls);
            await smtpClient.AuthenticateAsync(senderEmail, senderPassword);
            await smtpClient.SendAsync(emailMessage);
            await smtpClient.DisconnectAsync(true);
        }

        private static ActionResponse Fail(string message)
        {
            return new ActionResponse
            {
                IsSuccess = false,
                Message = message
            };
        }
    }
}
