using System.Collections.Generic;
using DependencyTracker.Data.Models;

namespace DependencyTracker.Data.Services
{
    public interface IPackageService
    {
        Package GetById(int id);
        Package GetByTypeAndName(int packageTypeId, string name);
        Package GetOrCreateByTypeAndName(int packageTypeId, string name, string user);
        Package Create(Package package, string user);
        void Delete(int packageId, string user);
        IEnumerable<ApplicationPackageMapping> GetMappingsForApplication(int applicationId);
        void SetForApplication(int applicationId, IEnumerable<ApplicationPackageInput> inputs, string user);
    }
}