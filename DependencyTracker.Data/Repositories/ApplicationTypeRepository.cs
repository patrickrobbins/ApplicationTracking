using System.Collections.Generic;
using System.Linq;
using DependencyTracker.Data.Models;

namespace DependencyTracker.Data.Repositories
{
    public class ApplicationTypeRepository : Repository<ApplicationType>, IApplicationTypeRepository
    {
        public ApplicationTypeRepository() : base() { }

        public ApplicationTypeRepository(DependencyTrackerDbContext context) : base(context) { }

        public IEnumerable<ApplicationType> GetApplicationTypes()
        {
            return DbSet
                .OrderBy(t => t.SortOrder)
                .ThenBy(t => t.Name)
                .ToList();
        }

        public IEnumerable<ApplicationType> GetActiveApplicationTypes()
        {
            return DbSet
                .Where(t => t.IsActive)
                .OrderBy(t => t.SortOrder)
                .ThenBy(t => t.Name)
                .ToList();
        }

        public ApplicationType GetByName(string name)
        {
            return DbSet.FirstOrDefault(t => t.Name == name);
        }
    }
}