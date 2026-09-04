using System;
using System.Collections.Generic;
using System.Linq;
using DependencyTracker.Data.Models;
using DependencyTracker.Data.Repositories;

namespace DependencyTracker.Data.Services
{
    public class TechnicalOwnershipTeamService : ITechnicalOwnershipTeamService
    {
        private readonly ITechnicalOwnershipTeamRepository _teamRepository;
        private readonly IApplicationRepository _applicationRepository;
        private readonly IAdminRepository _adminRepository;

        public TechnicalOwnershipTeamService()
            : this(new TechnicalOwnershipTeamRepository(), new ApplicationRepository(), new AdminRepository())
        {
        }

        public TechnicalOwnershipTeamService(
            ITechnicalOwnershipTeamRepository teamRepository,
            IApplicationRepository applicationRepository,
            IAdminRepository adminRepository)
        {
            _teamRepository = teamRepository;
            _applicationRepository = applicationRepository;
            _adminRepository = adminRepository;
        }

        public IEnumerable<TechnicalOwnershipTeam> GetTeams()
        {
            return _teamRepository.GetTeams();
        }

        public IEnumerable<TechnicalOwnershipTeam> GetActiveTeams()
        {
            return _teamRepository.GetActiveTeams();
        }

        public TechnicalOwnershipTeam GetById(int id)
        {
            return _teamRepository.GetById(id);
        }

        public TechnicalOwnershipTeam Create(TechnicalOwnershipTeam team, string user)
        {
            if (_teamRepository.GetByName(team.Name) != null)
                throw new InvalidOperationException($"A team named '{team.Name}' already exists.");

            team.IsActive = true;
            team.CreatedDate = DateTime.UtcNow;
            team.ModifiedDate = DateTime.UtcNow;
            team.CreatedBy = user;
            team.ModifiedBy = user;

            var created = _teamRepository.Add(team);
            _teamRepository.SaveChanges();

            _adminRepository.Log("Created", "TechnicalOwnershipTeam", created.TeamId, created.Name,
                $"Team '{created.Name}' created", user);
            _adminRepository.SaveChanges();

            return created;
        }

        public TechnicalOwnershipTeam Update(TechnicalOwnershipTeam team, string user)
        {
            var existing = _teamRepository.GetById(team.TeamId);
            if (existing == null)
                throw new InvalidOperationException($"Team {team.TeamId} not found.");

            var duplicate = _teamRepository.GetByName(team.Name);
            if (duplicate != null && duplicate.TeamId != team.TeamId)
                throw new InvalidOperationException($"A team named '{team.Name}' already exists.");

            existing.Name = team.Name;
            existing.Description = team.Description;
            existing.IsActive = team.IsActive;
            existing.SortOrder = team.SortOrder;
            existing.ModifiedDate = DateTime.UtcNow;
            existing.ModifiedBy = user;

            _teamRepository.SaveChanges();

            _adminRepository.Log("Updated", "TechnicalOwnershipTeam", existing.TeamId, existing.Name,
                $"Team '{existing.Name}' updated", user);
            _adminRepository.SaveChanges();

            return existing;
        }

        public void ToggleActive(int teamId, string user)
        {
            var team = _teamRepository.GetById(teamId);
            if (team == null)
                return;

            team.IsActive = !team.IsActive;
            team.ModifiedDate = DateTime.UtcNow;
            team.ModifiedBy = user;
            _teamRepository.SaveChanges();

            _adminRepository.Log("Updated", "TechnicalOwnershipTeam", teamId, team.Name,
                $"Team '{team.Name}' {(team.IsActive ? "activated" : "deactivated")}", user);
            _adminRepository.SaveChanges();
        }

        public void Delete(int teamId, string user)
        {
            var team = _teamRepository.GetById(teamId);
            if (team == null)
                return;

            // Detach applications from the team first so the FK is not
            // violated (the DB also has ON DELETE SET NULL as a backstop).
            var applications = _applicationRepository.Query()
                .Where(a => a.TechnicalOwnershipTeamId == teamId)
                .ToList();
            foreach (var application in applications)
            {
                application.TechnicalOwnershipTeamId = null;
                application.ModifiedDate = DateTime.UtcNow;
                application.ModifiedBy = user;
            }
            _applicationRepository.SaveChanges();

            _adminRepository.Log("Deleted", "TechnicalOwnershipTeam", teamId, team.Name,
                $"Team '{team.Name}' removed", user);
            _adminRepository.SaveChanges();

            _teamRepository.Remove(team);
            _teamRepository.SaveChanges();
        }
    }
}