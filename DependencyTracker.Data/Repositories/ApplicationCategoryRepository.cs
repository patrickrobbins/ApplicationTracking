using System.Collections.Generic;
using System.Linq;
using DependencyTracker.Data.Models;

namespace DependencyTracker.Data.Repositories
{
    public class ApplicationCategoryRepository : Repository<ApplicationCategory>, IApplicationCategoryRepository
    {
        public ApplicationCategoryRepository() : base() { }

        public ApplicationCategoryRepository(DependencyTrackerDbContext context) : base(context) { }

        public IEnumerable<ApplicationCategory> GetCategories()
        {
            return DbSet
                .OrderBy(c => c.SortOrder)
                .ThenBy(c => c.Name)
                .ToList();
        }

        public IEnumerable<ApplicationCategory> GetActiveCategories()
        {
            return DbSet
                .Where(c => c.IsActive)
                .OrderBy(c => c.SortOrder)
                .ThenBy(c => c.Name)
                .ToList();
        }

        public ApplicationCategory GetByName(string name)
        {
            return DbSet.FirstOrDefault(c => c.Name == name);
        }
    }
}
