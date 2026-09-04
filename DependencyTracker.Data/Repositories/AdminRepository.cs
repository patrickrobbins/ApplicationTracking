using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using DependencyTracker.Data.Models;

namespace DependencyTracker.Data.Repositories
{
    public class AdminRepository : Repository<AdGroup>, IAdminRepository
    {
        public AdminRepository() : base() { }

        public AdminRepository(DependencyTrackerDbContext context) : base(context) { }

        public IEnumerable<AdGroup> GetActiveGroups()
        {
            return DbSet.Where(g => g.IsActive).OrderBy(g => g.Role).ThenBy(g => g.GroupName).ToList();
        }

        public IEnumerable<AdGroup> GetGroupsByRole(string role)
        {
            return DbSet.Where(g => g.Role == role && g.IsActive).OrderBy(g => g.GroupName).ToList();
        }

        public AdGroup GetByName(string groupName)
        {
            return DbSet.FirstOrDefault(g => g.GroupName == groupName);
        }

        public IEnumerable<ActivityLogEntry> GetActivityLog(int take = 200)
        {
            return Context.ActivityLog
                .OrderByDescending(l => l.PerformedDate)
                .Take(take)
                .ToList();
        }

        public IEnumerable<ActivityLogEntry> SearchActivityLog(string entityType, string action, string user, int take = 500)
        {
            var query = Context.ActivityLog.AsQueryable();

            if (!string.IsNullOrWhiteSpace(entityType))
                query = query.Where(l => l.EntityType == entityType);

            if (!string.IsNullOrWhiteSpace(action))
                query = query.Where(l => l.Action == action);

            if (!string.IsNullOrWhiteSpace(user))
                query = query.Where(l => l.PerformedBy.Contains(user));

            return query.OrderByDescending(l => l.PerformedDate).Take(take).ToList();
        }

        public void Log(string action, string entityType, int entityId, string entityName, string details, string performedBy)
        {
            Context.ActivityLog.Add(new ActivityLogEntry
            {
                Action = action,
                EntityType = entityType,
                EntityId = entityId,
                EntityName = entityName,
                Details = details,
                PerformedBy = performedBy,
                PerformedDate = System.DateTime.UtcNow
            });
        }

        public IDictionary<string, int> GetRoleAssignments()
        {
            return DbSet
                .Where(g => g.IsActive)
                .GroupBy(g => g.Role)
                .ToDictionary(g => g.Key, g => g.Count());
        }
    }
}
