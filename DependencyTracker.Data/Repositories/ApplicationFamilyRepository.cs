using System.Collections.Generic;
using System.Linq;
using DependencyTracker.Data.Models;

namespace DependencyTracker.Data.Repositories
{
    public class ApplicationFamilyRepository : Repository<ApplicationFamily>, IApplicationFamilyRepository
    {
        public ApplicationFamilyRepository() : base() { }

        public ApplicationFamilyRepository(DependencyTrackerDbContext context) : base(context) { }

        public IEnumerable<ApplicationFamily> GetFamilies()
        {
            return DbSet
                .OrderBy(f => f.SortOrder)
                .ThenBy(f => f.Name)
                .ToList();
        }

        public IEnumerable<ApplicationFamily> GetActiveFamilies()
        {
            return DbSet
                .Where(f => f.IsActive)
                .OrderBy(f => f.SortOrder)
                .ThenBy(f => f.Name)
                .ToList();
        }

        public ApplicationFamily GetByName(string name)
        {
            return DbSet.FirstOrDefault(f => f.Name == name);
        }
    }
}