using System.Collections.Generic;
using DependencyTracker.Data.Models;

namespace DependencyTracker.Data.Repositories
{
    public interface IApplicationFamilyRepository : IRepository<ApplicationFamily>
    {
        IEnumerable<ApplicationFamily> GetFamilies();
        IEnumerable<ApplicationFamily> GetActiveFamilies();
        ApplicationFamily GetByName(string name);
    }
}