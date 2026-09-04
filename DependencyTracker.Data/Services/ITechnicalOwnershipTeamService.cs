using System.Collections.Generic;
using DependencyTracker.Data.Models;

namespace DependencyTracker.Data.Services
{
    public interface ITechnicalOwnershipTeamService
    {
        IEnumerable<TechnicalOwnershipTeam> GetTeams();
        IEnumerable<TechnicalOwnershipTeam> GetActiveTeams();
        TechnicalOwnershipTeam GetById(int id);
        TechnicalOwnershipTeam Create(TechnicalOwnershipTeam team, string user);
        TechnicalOwnershipTeam Update(TechnicalOwnershipTeam team, string user);
        void ToggleActive(int teamId, string user);
        void Delete(int teamId, string user);
    }
}