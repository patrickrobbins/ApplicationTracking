using System.Collections.Generic;
using DependencyTracker.Data.Models;

namespace DependencyTracker.Data.Services
{
    public interface IAdminService
    {
        IEnumerable<AdGroup> GetActiveGroups();
        IEnumerable<AdGroup> GetAllGroups();
        AdGroup GetGroupById(int id);
        AdGroup CreateGroup(AdGroup group, string user);
        AdGroup UpdateGroup(AdGroup group, string user);
        void DeleteGroup(int groupId, string user);
        void ToggleGroupActive(int groupId, string user);
        IEnumerable<ActivityLogEntry> GetActivityLog(int take = 200);
        IEnumerable<ActivityLogEntry> SearchActivityLog(string entityType, string action, string user);
        IDictionary<string, int> GetRoleAssignments();
    }
}
