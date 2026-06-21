using Microsoft.Extensions.Logging;
using Quartz;

namespace LibraryManagement.BLL.Services.Jobs
{
    public class DueReminderJob : IJob
    {
        private readonly DueReminderService dueReminderService;
        private readonly ILogger<DueReminderJob> logger;

        public DueReminderJob(DueReminderService _dueReminderService, ILogger<DueReminderJob> _logger)
        {
            dueReminderService = _dueReminderService;
            logger = _logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var sentCount = await dueReminderService.SendDueRemindersAsync();
            logger.LogInformation("Due reminder job completed. Sent {SentCount} reminders.", sentCount);
        }
    }
}
