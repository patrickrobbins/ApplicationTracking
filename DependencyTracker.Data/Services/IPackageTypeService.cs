using System.Collections.Generic;
using DependencyTracker.Data.Models;

namespace DependencyTracker.Data.Services
{
    public interface IPackageTypeService
    {
        IEnumerable<PackageType> GetPackageTypes();
        IEnumerable<PackageType> GetActivePackageTypes();
        PackageType GetById(int id);
        PackageType Create(PackageType packageType, string user);
        PackageType Update(PackageType packageType, string user);
        void ToggleActive(int packageTypeId, string user);
        void Delete(int packageTypeId, string user);
    }
}