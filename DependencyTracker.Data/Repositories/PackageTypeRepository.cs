using System.Collections.Generic;
using System.Linq;
using DependencyTracker.Data.Models;

namespace DependencyTracker.Data.Repositories
{
    public class PackageTypeRepository : Repository<PackageType>, IPackageTypeRepository
    {
        public PackageTypeRepository() : base() { }

        public PackageTypeRepository(DependencyTrackerDbContext context) : base(context) { }

        public IEnumerable<PackageType> GetPackageTypes()
        {
            return DbSet
                .OrderBy(t => t.SortOrder)
                .ThenBy(t => t.Name)
                .ToList();
        }

        public IEnumerable<PackageType> GetActivePackageTypes()
        {
            return DbSet
                .Where(t => t.IsActive)
                .OrderBy(t => t.SortOrder)
                .ThenBy(t => t.Name)
                .ToList();
        }

        public PackageType GetByName(string name)
        {
            return DbSet.FirstOrDefault(t => t.Name == name);
        }
    }
}