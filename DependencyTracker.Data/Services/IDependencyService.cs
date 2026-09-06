using System.Collections.Generic;
using DependencyTracker.Data.Models;

namespace DependencyTracker.Data.Services
{
    public interface IDependencyService
    {
        Dependency GetById(int id);
        IEnumerable<Dependency> GetAll();
        IEnumerable<Dependency> GetForApplication(int applicationId);
        IEnumerable<Dependency> GetUpstream(int applicationId);
        IEnumerable<Dependency> GetDownstream(int applicationId);
        IEnumerable<Dependency> Search(string dependencyType, string criticality, int? sourceId, int? targetId);
        Dependency Create(Dependency dependency, string user);
        Dependency Update(Dependency dependency, string user);
        void Delete(int dependencyId, string user);
        bool RelationshipExists(int sourceId, int targetId, string dependencyType);
        IDictionary<int, int> GetCountsForApplications(IEnumerable<int> applicationIds);
        IEnumerable<DependencyChainResult> GetChain(int applicationId, int maxDepth, string direction);
        IEnumerable<string> GetAllTypes();
        IEnumerable<string> GetAllFrequencies();
        IEnumerable<string> GetAllDirections();
        IEnumerable<string> GetAllCriticalityLevels();
    }
}
