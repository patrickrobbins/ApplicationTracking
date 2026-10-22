using System.Collections.Generic;
using System.Linq;
using DependencyTracker.Data.Models;

namespace DependencyTracker.Data.Repositories
{
    public interface IDependencyRepository : IRepository<Dependency>
    {
        IEnumerable<Dependency> GetForApplication(int applicationId);
        IEnumerable<Dependency> GetUpstream(int applicationId);
        IEnumerable<Dependency> GetDownstream(int applicationId);
        IEnumerable<Dependency> Search(string dependencyType, string criticality, int? sourceId, int? targetId);
        bool RelationshipExists(int sourceId, int targetId, string dependencyType);

        /// <summary>
        /// True when the same source/target/type relationship exists on a
        /// dependency other than <paramref name="excludeDependencyId"/>. Used
        /// when updating so a dependency does not collide with itself.
        /// </summary>
        bool RelationshipExistsExcluding(int sourceId, int targetId, string dependencyType, int excludeDependencyId);
        int CountForApplication(int applicationId);
        IDictionary<int, int> GetCountsForApplications(IEnumerable<int> applicationIds);
        IEnumerable<DependencyChainResult> GetChain(int applicationId, int maxDepth, string direction);
    }
}
