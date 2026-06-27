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
using System.Net;

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

        public async Task MarkAsReadAsync(int notificationId)
        {
            await notificationRepository.MarkAsReadAsync(notificationId);
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
            emailMessage.Body = new BodyBuilder
            {
                TextBody = $"Hello {fullName},\n\n{message}\n\nLMS Standard",
                HtmlBody = BuildHtmlEmail(fullName, title, message)
            }.ToMessageBody();

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

        private static string BuildHtmlEmail(string fullName, string title, string message)
        {
            var safeName = WebUtility.HtmlEncode(fullName);
            var safeTitle = WebUtility.HtmlEncode(title);
            var safeMessage = WebUtility.HtmlEncode(message).Replace("\n", "<br>");

            return $"""
                <div style="margin:0;padding:32px;background:#071f1c;font-family:Arial,sans-serif;color:#f8fafc">
                  <div style="max-width:560px;margin:0 auto;background:#0b1220;border:1px solid rgba(255,255,255,.12);border-radius:18px;overflow:hidden">
                    <div style="padding:24px 28px;background:#0f302b;border-bottom:1px solid rgba(255,255,255,.1)">
                      <div style="font-size:20px;font-weight:800;color:#ffea00">LMS Standard</div>
                    </div>
                    <div style="padding:28px">
                      <p style="margin:0 0 12px;color:#94a3b8">Hello {safeName},</p>
                      <h1 style="margin:0 0 16px;font-size:24px;line-height:1.25;color:#ffffff">{safeTitle}</h1>
                      <p style="margin:0;color:#cbd5e1;font-size:15px;line-height:1.7">{safeMessage}</p>
                    </div>
                    <div style="padding:18px 28px;color:#64748b;font-size:12px;border-top:1px solid rgba(255,255,255,.1)">
                      This is an automated notification from LMS Standard.
                    </div>
                  </div>
                </div>
                """;
        }
    }
}
