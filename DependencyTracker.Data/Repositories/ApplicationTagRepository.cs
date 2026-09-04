using System.Collections.Generic;
using System.Linq;
using DependencyTracker.Data.Models;

namespace DependencyTracker.Data.Repositories
{
    public class ApplicationTagRepository : Repository<ApplicationTag>, IApplicationTagRepository
    {
        public ApplicationTagRepository() : base() { }

        public ApplicationTagRepository(DependencyTrackerDbContext context) : base(context) { }

        public IEnumerable<ApplicationTag> GetTags()
        {
            return DbSet
                .OrderBy(t => t.SortOrder)
                .ThenBy(t => t.Name)
                .ToList();
        }

        public IEnumerable<ApplicationTag> GetActiveTags()
        {
            return DbSet
                .Where(t => t.IsActive)
                .OrderBy(t => t.SortOrder)
                .ThenBy(t => t.Name)
                .ToList();
        }

        public ApplicationTag GetByName(string name)
        {
            return DbSet.FirstOrDefault(t => t.Name == name);
        }

        public IEnumerable<ApplicationTag> GetForApplication(int applicationId)
        {
            return Context.ApplicationTagMappings
                .Where(m => m.ApplicationId == applicationId)
                .Select(m => m.Tag)
                .OrderBy(t => t.SortOrder)
                .ThenBy(t => t.Name)
                .ToList();
        }

        public ApplicationTagMapping GetMapping(int applicationId, int tagId)
        {
            return Context.ApplicationTagMappings
                .FirstOrDefault(m => m.ApplicationId == applicationId && m.TagId == tagId);
        }

        public ApplicationTagMapping AddMapping(int applicationId, int tagId)
        {
            return Context.ApplicationTagMappings.Add(new ApplicationTagMapping
            {
                ApplicationId = applicationId,
                TagId = tagId
            });
        }

        public void RemoveMapping(ApplicationTagMapping mapping)
        {
            Context.ApplicationTagMappings.Remove(mapping);
        }

        public IEnumerable<ApplicationTagMapping> GetMappingsForApplication(int applicationId)
        {
            return Context.ApplicationTagMappings
                .Where(m => m.ApplicationId == applicationId)
                .ToList();
        }

        public IEnumerable<ApplicationTagMapping> GetMappingsForTag(int tagId)
        {
            return Context.ApplicationTagMappings
                .Where(m => m.TagId == tagId)
                .ToList();
        }
    }
}