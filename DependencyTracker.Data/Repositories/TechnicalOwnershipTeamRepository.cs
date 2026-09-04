using System.Collections.Generic;
using System.Linq;
using DependencyTracker.Data.Models;

namespace DependencyTracker.Data.Repositories
{
    public class TechnicalOwnershipTeamRepository : Repository<TechnicalOwnershipTeam>, ITechnicalOwnershipTeamRepository
    {
        public TechnicalOwnershipTeamRepository() : base() { }

        public TechnicalOwnershipTeamRepository(DependencyTrackerDbContext context) : base(context) { }

        public IEnumerable<TechnicalOwnershipTeam> GetTeams()
        {
            return DbSet
                .OrderBy(t => t.SortOrder)
                .ThenBy(t => t.Name)
                .ToList();
        }

        public IEnumerable<TechnicalOwnershipTeam> GetActiveTeams()
        {
            return DbSet
                .Where(t => t.IsActive)
                .OrderBy(t => t.SortOrder)
                .ThenBy(t => t.Name)
                .ToList();
        }

        public TechnicalOwnershipTeam GetByName(string name)
        {
            return DbSet.FirstOrDefault(t => t.Name == name);
        }
    }
}