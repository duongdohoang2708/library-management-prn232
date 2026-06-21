using LibraryManagement.BLL.DTO.Audit;
using LibraryManagement.BLL.DTO.User;
using LibraryManagement.DAL.Data;
using LibraryManagement.DAL.Repositories;
using LibraryManagementDAL.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LibraryManagement.BLL.Services
{
    public class AuditLogService
    {
        private const int PageSize = 20;
        private readonly AuditLogRepository auditLogRepository;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly ApplicationDbContext db;

        public AuditLogService(AuditLogRepository _auditLogRepository, IHttpContextAccessor _httpContextAccessor, ApplicationDbContext _db)
        {
            auditLogRepository = _auditLogRepository;
            httpContextAccessor = _httpContextAccessor;
            db = _db;
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

            var items = await query.Skip((page - 1) * PageSize).Take(PageSize).ToListAsync();

            var actorUserIds = items.Where(x => x.ActorUserId.HasValue).Select(x => x.ActorUserId!.Value).Distinct().ToList();
            var userRolesMap = new Dictionary<int, string>();

            if (actorUserIds.Any())
            {
                var accounts = await db.Accounts
                    .AsNoTracking()
                    .Include(a => a.Staff).ThenInclude(s => s.Role)
                    .Include(a => a.Member)
                    .Where(a => actorUserIds.Contains(a.UserId))
                    .ToListAsync();

                foreach (var acc in accounts)
                {
                    string role = "User";
                    if (acc.Staff != null && acc.Staff.Role != null)
                    {
                        role = acc.Staff.Role.RoleName;
                    }
                    else if (acc.Member != null)
                    {
                        role = "Member";
                    }
                    userRolesMap[acc.UserId] = role;
                }
            }

            var auditLogItems = items.Select(x => new AuditLogItem
            {
                AuditLogId = x.AuditLogId,
                ActorUserId = x.ActorUserId,
                ActorName = x.ActorName,
                ActorRole = x.ActorUserId.HasValue && userRolesMap.TryGetValue(x.ActorUserId.Value, out var role) ? role : "System/API",
                Action = x.Action,
                EntityName = x.EntityName,
                EntityId = x.EntityId,
                Summary = x.Summary,
                CreatedAt = x.CreatedAt
            }).ToList();

            return new PaginatedResult<AuditLogItem>
            {
                Items = auditLogItems,
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
            var resolvedActor = ResolveActor(actorUserId, actorName);

            await auditLogRepository.AddAsync(new AuditLog
            {
                ActorUserId = resolvedActor.ActorUserId,
                ActorName = resolvedActor.ActorName,
                Action = action,
                EntityName = entityName,
                EntityId = entityId,
                Summary = summary,
                CreatedAt = DateTime.UtcNow
            });
        }

        private (int? ActorUserId, string ActorName) ResolveActor(int? actorUserId, string? actorName)
        {
            if (actorUserId.HasValue || !string.IsNullOrWhiteSpace(actorName))
            {
                return (actorUserId, string.IsNullOrWhiteSpace(actorName) ? "System/API" : actorName.Trim());
            }

            var context = httpContextAccessor.HttpContext;
            if (context == null)
            {
                return (null, "System/API");
            }

            int? resolvedUserId = null;
            if (context.Request.Headers.TryGetValue("X-Actor-UserId", out var headerUserId)
                && int.TryParse(headerUserId.FirstOrDefault(), out var parsedHeaderUserId))
            {
                resolvedUserId = parsedHeaderUserId;
            }
            else if (int.TryParse(context.User.FindFirstValue(ClaimTypes.NameIdentifier), out var parsedClaimUserId))
            {
                resolvedUserId = parsedClaimUserId;
            }

            var resolvedName = context.Request.Headers.TryGetValue("X-Actor-Name", out var headerActorName)
                ? headerActorName.FirstOrDefault()
                : null;

            resolvedName = string.IsNullOrWhiteSpace(resolvedName)
                ? context.User.FindFirstValue("FullName")
                    ?? context.User.FindFirstValue(ClaimTypes.Name)
                    ?? context.User.FindFirstValue(ClaimTypes.Email)
                : resolvedName;

            return (resolvedUserId, string.IsNullOrWhiteSpace(resolvedName) ? "System/API" : resolvedName.Trim());
        }
    }
}
