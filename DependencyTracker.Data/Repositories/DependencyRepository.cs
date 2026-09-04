using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using DependencyTracker.Data.Models;

namespace DependencyTracker.Data.Repositories
{
    public class DependencyRepository : Repository<Dependency>, IDependencyRepository
    {
        public DependencyRepository() : base() { }

        public DependencyRepository(DependencyTrackerDbContext context) : base(context) { }

        public IEnumerable<Dependency> GetForApplication(int applicationId)
        {
            return DbSet
                .Include(d => d.SourceApplication)
                .Include(d => d.TargetApplication)
                .Include(d => d.SourceApplication.Category)
                .Include(d => d.TargetApplication.Category)
                .Where(d =>
                    (d.SourceApplicationId == applicationId || d.TargetApplicationId == applicationId) &&
                    !d.SourceApplication.IsDeleted &&
                    !d.TargetApplication.IsDeleted)
                .OrderBy(d => d.DependencyType)
                .ToList();
        }

        public IEnumerable<Dependency> GetUpstream(int applicationId)
        {
            // What this application depends on, ordered by the target's application
            // type (category) then name.
            return DbSet
                .Include(d => d.SourceApplication)
                .Include(d => d.TargetApplication)
                .Include(d => d.TargetApplication.Category)
                .Where(d => d.SourceApplicationId == applicationId && !d.TargetApplication.IsDeleted)
                .OrderBy(d => d.TargetApplication.Category.Name)
                .ThenBy(d => d.TargetApplication.Name)
                .ToList();
        }

        public IEnumerable<Dependency> GetDownstream(int applicationId)
        {
            // What depends on this application, ordered by the source's application
            // type (category) then name.
            return DbSet
                .Include(d => d.SourceApplication)
                .Include(d => d.TargetApplication)
                .Include(d => d.SourceApplication.Category)
                .Where(d => d.TargetApplicationId == applicationId && !d.SourceApplication.IsDeleted)
                .OrderBy(d => d.SourceApplication.Category.Name)
                .ThenBy(d => d.SourceApplication.Name)
                .ToList();
        }

        public IEnumerable<Dependency> Search(string dependencyType, string criticality, int? sourceId, int? targetId)
        {
            var query = DbSet
                .Include(d => d.SourceApplication)
                .Include(d => d.TargetApplication)
                .Where(d => !d.SourceApplication.IsDeleted && !d.TargetApplication.IsDeleted)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(dependencyType))
                query = query.Where(d => d.DependencyType == dependencyType);

            if (!string.IsNullOrWhiteSpace(criticality))
                query = query.Where(d => d.CriticalityLevel == criticality);

            if (sourceId.HasValue)
                query = query.Where(d => d.SourceApplicationId == sourceId.Value);

            if (targetId.HasValue)
                query = query.Where(d => d.TargetApplicationId == targetId.Value);

            return query
                .OrderBy(d => d.SourceApplication.Name)
                .ThenBy(d => d.TargetApplication.Name)
                .ToList();
        }

        public bool RelationshipExists(int sourceId, int targetId, string dependencyType)
        {
            return DbSet.Any(d =>
                d.SourceApplicationId == sourceId &&
                d.TargetApplicationId == targetId &&
                d.DependencyType == dependencyType);
        }

        public int CountForApplication(int applicationId)
        {
            return DbSet.Count(d => d.SourceApplicationId == applicationId || d.TargetApplicationId == applicationId);
        }

        public IEnumerable<DependencyChainResult> GetChain(int applicationId, int maxDepth, string direction)
        {
            return Context.GetDependencyChain(applicationId, maxDepth, direction).ToList();
        }
    }
}
