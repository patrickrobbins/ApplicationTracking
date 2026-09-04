using System.Collections.Generic;
using System.Linq;
using DependencyTracker.Data.Models;

namespace DependencyTracker.Data.Repositories
{
    public class ApplicationTechnologyRepository : Repository<ApplicationTechnology>, IApplicationTechnologyRepository
    {
        public ApplicationTechnologyRepository() : base() { }

        public ApplicationTechnologyRepository(DependencyTrackerDbContext context) : base(context) { }

        public IEnumerable<ApplicationTechnology> GetTechnologies()
        {
            return DbSet
                .OrderBy(t => t.SortOrder)
                .ThenBy(t => t.Name)
                .ToList();
        }

        public IEnumerable<ApplicationTechnology> GetActiveTechnologies()
        {
            return DbSet
                .Where(t => t.IsActive)
                .OrderBy(t => t.SortOrder)
                .ThenBy(t => t.Name)
                .ToList();
        }

        public ApplicationTechnology GetByName(string name)
        {
            return DbSet.FirstOrDefault(t => t.Name == name);
        }

        public IEnumerable<ApplicationTechnology> GetForApplication(int applicationId)
        {
            return Context.ApplicationTechnologyMappings
                .Where(m => m.ApplicationId == applicationId)
                .Select(m => m.Technology)
                .OrderBy(t => t.SortOrder)
                .ThenBy(t => t.Name)
                .ToList();
        }

        public ApplicationTechnologyMapping GetMapping(int applicationId, int technologyId)
        {
            return Context.ApplicationTechnologyMappings
                .FirstOrDefault(m => m.ApplicationId == applicationId && m.TechnologyId == technologyId);
        }

        public ApplicationTechnologyMapping AddMapping(int applicationId, int technologyId)
        {
            return Context.ApplicationTechnologyMappings.Add(new ApplicationTechnologyMapping
            {
                ApplicationId = applicationId,
                TechnologyId = technologyId
            });
        }

        public void RemoveMapping(ApplicationTechnologyMapping mapping)
        {
            Context.ApplicationTechnologyMappings.Remove(mapping);
        }

        public IEnumerable<ApplicationTechnologyMapping> GetMappingsForApplication(int applicationId)
        {
            return Context.ApplicationTechnologyMappings
                .Where(m => m.ApplicationId == applicationId)
                .ToList();
        }

        public IEnumerable<ApplicationTechnologyMapping> GetMappingsForTechnology(int technologyId)
        {
            return Context.ApplicationTechnologyMappings
                .Where(m => m.TechnologyId == technologyId)
                .ToList();
        }
    }
}
