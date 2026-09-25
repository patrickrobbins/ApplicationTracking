using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using DependencyTracker.Data.Models;

namespace DependencyTracker.Data.Repositories
{
    public class PackageRepository : Repository<Package>, IPackageRepository
    {
        public PackageRepository() : base() { }

        public PackageRepository(DependencyTrackerDbContext context) : base(context) { }

        public IEnumerable<Package> GetPackages()
        {
            return DbSet
                .OrderBy(p => p.PackageType.SortOrder)
                .ThenBy(p => p.PackageType.Name)
                .ThenBy(p => p.Name)
                .ToList();
        }

        public IEnumerable<Package> GetActivePackages()
        {
            return DbSet
                .Where(p => p.IsActive)
                .OrderBy(p => p.PackageType.SortOrder)
                .ThenBy(p => p.PackageType.Name)
                .ThenBy(p => p.Name)
                .ToList();
        }

        public Package GetByTypeAndName(int packageTypeId, string name)
        {
            return DbSet.FirstOrDefault(p => p.PackageTypeId == packageTypeId && p.Name == name);
        }

        public IEnumerable<Package> GetForApplication(int applicationId)
        {
            return Context.ApplicationPackageMappings
                .Where(m => m.ApplicationId == applicationId)
                .Select(m => m.Package)
                .OrderBy(p => p.PackageType.SortOrder)
                .ThenBy(p => p.PackageType.Name)
                .ThenBy(p => p.Name)
                .ToList();
        }

        public ApplicationPackageMapping GetMapping(int applicationId, int packageId)
        {
            return Context.ApplicationPackageMappings
                .FirstOrDefault(m => m.ApplicationId == applicationId && m.PackageId == packageId);
        }

        public ApplicationPackageMapping AddMapping(int applicationId, int packageId, string version)
        {
            return Context.ApplicationPackageMappings.Add(new ApplicationPackageMapping
            {
                ApplicationId = applicationId,
                PackageId = packageId,
                Version = version
            });
        }

        public void RemoveMapping(ApplicationPackageMapping mapping)
        {
            Context.ApplicationPackageMappings.Remove(mapping);
        }

        public IEnumerable<ApplicationPackageMapping> GetMappingsForApplication(int applicationId)
        {
            return Context.ApplicationPackageMappings
                .Include(m => m.Package.PackageType)
                .Where(m => m.ApplicationId == applicationId)
                .OrderBy(m => m.Package.PackageType.SortOrder)
                .ThenBy(m => m.Package.PackageType.Name)
                .ThenBy(m => m.Package.Name)
                .ToList();
        }

        public IEnumerable<ApplicationPackageMapping> GetMappingsForPackage(int packageId)
        {
            return Context.ApplicationPackageMappings
                .Where(m => m.PackageId == packageId)
                .ToList();
        }
    }
}