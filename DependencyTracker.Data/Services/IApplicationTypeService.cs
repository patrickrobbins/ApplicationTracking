using System.Collections.Generic;
using DependencyTracker.Data.Models;

namespace DependencyTracker.Data.Services
{
    public interface IApplicationTypeService
    {
        IEnumerable<ApplicationType> GetApplicationTypes();
        IEnumerable<ApplicationType> GetActiveApplicationTypes();
        ApplicationType GetById(int id);
        ApplicationType Create(ApplicationType applicationType, string user);
        ApplicationType Update(ApplicationType applicationType, string user);
        void ToggleActive(int applicationTypeId, string user);
        void Delete(int applicationTypeId, string user);
    }
}