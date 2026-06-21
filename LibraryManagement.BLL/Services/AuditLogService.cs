using LibraryManagement.BLL.DTO.Audit;
using LibraryManagement.BLL.DTO.User;
using LibraryManagement.DAL.Repositories;
using LibraryManagementDAL.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.BLL.Services
{
    public class AuditLogService
    {
        private const int PageSize = 20;
        private readonly AuditLogRepository auditLogRepository;

        public AuditLogService(AuditLogRepository _auditLogRepository)
        {
            auditLogRepository = _auditLogRepository;
        }

        public async Task<PaginatedResult<AuditLogItem>> GetLogsAsync(string? search, int page)
        {
            page = page < 1 ? 1 : page;
            var query = auditLogRepository.Query();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var key = search.Trim();
                query = query.Where(x =>
                    x.ActorName.Contains(key) ||
                    x.Action.Contains(key) ||
                    x.EntityName.Contains(key) ||
                    x.Summary.Contains(key));
            }

            query = query.OrderByDescending(x => x.CreatedAt);
            var totalCount = await query.CountAsync();
            var totalPages = Math.Max(1, (int)Math.Ceiling(totalCount / (double)PageSize));
            page = Math.Min(page, totalPages);

            return new PaginatedResult<AuditLogItem>
            {
                Items = await query.Skip((page - 1) * PageSize).Take(PageSize)
                    .Select(x => new AuditLogItem
                    {
                        AuditLogId = x.AuditLogId,
                        ActorUserId = x.ActorUserId,
                        ActorName = x.ActorName,
                        Action = x.Action,
                        EntityName = x.EntityName,
                        EntityId = x.EntityId,
                        Summary = x.Summary,
                        CreatedAt = x.CreatedAt
                    })
                    .ToListAsync(),
                PageNumber = page,
                CurrentPage = page,
                PageSize = PageSize,
                TotalCount = totalCount,
                TotalPages = totalPages,
                HasPreviousPage = page > 1,
                HasNextPage = page < totalPages
            };
        }

        public async Task LogAsync(string action, string entityName, string? entityId, string summary, int? actorUserId = null, string? actorName = null)
        {
            await auditLogRepository.AddAsync(new AuditLog
            {
                ActorUserId = actorUserId,
                ActorName = string.IsNullOrWhiteSpace(actorName) ? "System/API" : actorName.Trim(),
                Action = action,
                EntityName = entityName,
                EntityId = entityId,
                Summary = summary,
                CreatedAt = DateTime.UtcNow
            });
        }
    }
}
