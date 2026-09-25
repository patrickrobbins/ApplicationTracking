using System.Collections.Generic;
using DependencyTracker.Data.Models;

namespace DependencyTracker.Data.Repositories
{
    public interface IApplicationTypeRepository : IRepository<ApplicationType>
    {
        IEnumerable<ApplicationType> GetApplicationTypes();
        IEnumerable<ApplicationType> GetActiveApplicationTypes();
        ApplicationType GetByName(string name);
    }
}