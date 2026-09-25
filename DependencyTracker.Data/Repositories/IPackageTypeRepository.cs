using System.Collections.Generic;
using DependencyTracker.Data.Models;

namespace DependencyTracker.Data.Repositories
{
    public interface IPackageTypeRepository : IRepository<PackageType>
    {
        IEnumerable<PackageType> GetPackageTypes();
        IEnumerable<PackageType> GetActivePackageTypes();
        PackageType GetByName(string name);
    }
}