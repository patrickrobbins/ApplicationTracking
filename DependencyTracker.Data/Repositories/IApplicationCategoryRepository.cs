using System.Collections.Generic;
using DependencyTracker.Data.Models;

namespace DependencyTracker.Data.Repositories
{
    public interface IApplicationCategoryRepository : IRepository<ApplicationCategory>
    {
        IEnumerable<ApplicationCategory> GetCategories();
        IEnumerable<ApplicationCategory> GetActiveCategories();
        ApplicationCategory GetByName(string name);
    }
}
