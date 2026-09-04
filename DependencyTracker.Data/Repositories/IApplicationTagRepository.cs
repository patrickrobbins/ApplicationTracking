using System.Collections.Generic;
using DependencyTracker.Data.Models;

namespace DependencyTracker.Data.Repositories
{
    public interface IApplicationTagRepository : IRepository<ApplicationTag>
    {
        IEnumerable<ApplicationTag> GetTags();
        IEnumerable<ApplicationTag> GetActiveTags();
        ApplicationTag GetByName(string name);
        IEnumerable<ApplicationTag> GetForApplication(int applicationId);
        ApplicationTagMapping GetMapping(int applicationId, int tagId);
        ApplicationTagMapping AddMapping(int applicationId, int tagId);
        void RemoveMapping(ApplicationTagMapping mapping);
        IEnumerable<ApplicationTagMapping> GetMappingsForApplication(int applicationId);
        IEnumerable<ApplicationTagMapping> GetMappingsForTag(int tagId);
    }
}