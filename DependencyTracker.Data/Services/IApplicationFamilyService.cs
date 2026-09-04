using System.Collections.Generic;
using DependencyTracker.Data.Models;

namespace DependencyTracker.Data.Services
{
    public interface IApplicationFamilyService
    {
        IEnumerable<ApplicationFamily> GetFamilies();
        IEnumerable<ApplicationFamily> GetActiveFamilies();
        ApplicationFamily GetById(int id);
        ApplicationFamily Create(ApplicationFamily family, string user);
        ApplicationFamily Update(ApplicationFamily family, string user);
        void ToggleActive(int familyId, string user);
        void Delete(int familyId, string user);
    }
}