using System.Collections.Generic;
using DependencyTracker.Data.Models;

namespace DependencyTracker.Data.Services
{
    public interface IApplicationTagService
    {
        IEnumerable<ApplicationTag> GetTags();
        IEnumerable<ApplicationTag> GetActiveTags();
        ApplicationTag GetById(int id);
        ApplicationTag GetByName(string name);
        ApplicationTag GetOrCreateByName(string name, string user);
        ApplicationTag Create(ApplicationTag tag, string user);
        ApplicationTag Update(ApplicationTag tag, string user);
        void ToggleActive(int tagId, string user);
        void Delete(int tagId, string user);
        IEnumerable<ApplicationTag> GetForApplication(int applicationId);
        void SetForApplication(int applicationId, IEnumerable<int> tagIds, string user);
    }
}