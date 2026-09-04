using System.Collections.Generic;
using System.Linq;
using DependencyTracker.Data.Models;

namespace DependencyTracker.Data.Repositories
{
    public interface IAdminRepository : IRepository<AdGroup>
    {
        IEnumerable<AdGroup> GetActiveGroups();
        IEnumerable<AdGroup> GetGroupsByRole(string role);
        AdGroup GetByName(string groupName);
        IEnumerable<ActivityLogEntry> GetActivityLog(int take = 200);
        IEnumerable<ActivityLogEntry> SearchActivityLog(string entityType, string action, string user, int take = 500);
        void Log(string action, string entityType, int entityId, string entityName, string details, string performedBy);
        IDictionary<string, int> GetRoleAssignments();
    }
}
