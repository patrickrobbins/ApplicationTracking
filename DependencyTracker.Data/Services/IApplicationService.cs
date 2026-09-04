using System.Collections.Generic;
using DependencyTracker.Data.Models;

namespace DependencyTracker.Data.Services
{
    public interface IApplicationService
    {
        Application GetById(int id);
        IEnumerable<Application> GetAll();
        IEnumerable<Application> GetAllIncludingDeleted();
        IEnumerable<Application> GetVersionsByName(string name);
        IEnumerable<Application> Search(string term, string environment, string status, string criticality, int? categoryId, string version, int? technologyId, bool includeDeleted);
        Application Create(Application application, string user);
        Application Update(Application application, string user);
        void SoftDelete(int applicationId, string user);
        void HardDelete(int applicationId, string user);
        Application Restore(int applicationId, string user);
        bool CanDelete(int applicationId);
        IEnumerable<string> GetAllEnvironments();
        IEnumerable<string> GetAllStatuses();
        IEnumerable<string> GetAllCriticalities();
        IEnumerable<string> GetAllVersions();
        int TotalCount();
    }
}
