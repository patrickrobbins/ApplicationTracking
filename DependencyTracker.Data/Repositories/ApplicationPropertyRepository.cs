using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using DependencyTracker.Data.Models;

namespace DependencyTracker.Data.Repositories
{
    public class ApplicationPropertyRepository : Repository<ApplicationPropertyDefinition>, IApplicationPropertyRepository
    {
        public ApplicationPropertyRepository() : base() { }

        public ApplicationPropertyRepository(DependencyTrackerDbContext context) : base(context) { }

        public IEnumerable<ApplicationPropertyDefinition> GetDefinitions()
        {
            return DbSet
                .OrderBy(d => d.SortOrder)
                .ThenBy(d => d.Label)
                .ToList();
        }

        public IEnumerable<ApplicationPropertyDefinition> GetActiveDefinitions()
        {
            return DbSet
                .Where(d => d.IsActive)
                .OrderBy(d => d.SortOrder)
                .ThenBy(d => d.Label)
                .ToList();
        }

        public ApplicationPropertyDefinition GetDefinitionByKey(string key)
        {
            return DbSet.FirstOrDefault(d => d.Key == key);
        }

        public IEnumerable<ApplicationPropertyValue> GetValuesForApplication(int applicationId)
        {
            return Context.ApplicationPropertyValues
                .Where(v => v.ApplicationId == applicationId)
                .Include(v => v.Definition)
                .OrderBy(v => v.Definition.SortOrder)
                .ThenBy(v => v.Definition.Label)
                .ToList();
        }

        public IEnumerable<ApplicationPropertyValue> GetAllValues()
        {
            return Context.ApplicationPropertyValues
                .Include(v => v.Definition)
                .ToList();
        }

        public ApplicationPropertyValue AddValue(ApplicationPropertyValue value)
        {
            return Context.ApplicationPropertyValues.Add(value);
        }
    }
}
