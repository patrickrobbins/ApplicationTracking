using System.Collections.Generic;
using DependencyTracker.Data.Models;

namespace DependencyTracker.Data.Services
{
    public interface IApplicationCategoryService
    {
        IEnumerable<ApplicationCategory> GetCategories();
        IEnumerable<ApplicationCategory> GetActiveCategories();
        ApplicationCategory GetById(int id);
        ApplicationCategory Create(ApplicationCategory category, string user);
        ApplicationCategory Update(ApplicationCategory category, string user);
        void ToggleActive(int categoryId, string user);
        void Delete(int categoryId, string user);
    }
}
