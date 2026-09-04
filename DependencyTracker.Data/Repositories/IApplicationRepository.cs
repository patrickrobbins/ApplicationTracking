using System.Collections.Generic;
using System.Linq;
using DependencyTracker.Data.Models;

namespace DependencyTracker.Data.Repositories
{
    public interface IApplicationRepository : IRepository<Application>
    {
        IEnumerable<Application> Search(string term, string environment, string status, string criticality, int? categoryId, string version, int? technologyId, bool includeDeleted);
        Application GetByName(string name);
        IEnumerable<Application> GetAllIncludingDeleted();
        IEnumerable<Application> GetVersionsByName(string name);
        IEnumerable<string> GetAllEnvironments();
        IEnumerable<string> GetAllStatuses();
        IEnumerable<string> GetAllCriticalities();
        IEnumerable<string> GetAllVersions();
    }
}
