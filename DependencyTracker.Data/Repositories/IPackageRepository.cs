using System.Collections.Generic;
using DependencyTracker.Data.Models;

namespace DependencyTracker.Data.Repositories
{
    public interface IPackageRepository : IRepository<Package>
    {
        IEnumerable<Package> GetPackages();
        IEnumerable<Package> GetActivePackages();
        Package GetByTypeAndName(int packageTypeId, string name);
        IEnumerable<Package> GetForApplication(int applicationId);
        ApplicationPackageMapping GetMapping(int applicationId, int packageId);
        ApplicationPackageMapping AddMapping(int applicationId, int packageId, string version);
        void RemoveMapping(ApplicationPackageMapping mapping);
        IEnumerable<ApplicationPackageMapping> GetMappingsForApplication(int applicationId);
        IEnumerable<ApplicationPackageMapping> GetMappingsForPackage(int packageId);
    }
}