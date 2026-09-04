using System.Collections.Generic;
using DependencyTracker.Data.Models;

namespace DependencyTracker.Data.Repositories
{
    public interface ITechnicalOwnershipTeamRepository : IRepository<TechnicalOwnershipTeam>
    {
        IEnumerable<TechnicalOwnershipTeam> GetTeams();
        IEnumerable<TechnicalOwnershipTeam> GetActiveTeams();
        TechnicalOwnershipTeam GetByName(string name);
    }
}