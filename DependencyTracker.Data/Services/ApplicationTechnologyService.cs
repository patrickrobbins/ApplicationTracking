using System;
using System.Collections.Generic;
using System.Linq;
using DependencyTracker.Data.Models;
using DependencyTracker.Data.Repositories;

namespace DependencyTracker.Data.Services
{
    public class ApplicationTechnologyService : IApplicationTechnologyService
    {
        private readonly IApplicationTechnologyRepository _technologyRepository;
        private readonly IAdminRepository _adminRepository;

        public ApplicationTechnologyService()
            : this(new ApplicationTechnologyRepository(), new AdminRepository())
        {
        }

        public ApplicationTechnologyService(
            IApplicationTechnologyRepository technologyRepository,
            IAdminRepository adminRepository)
        {
            _technologyRepository = technologyRepository;
            _adminRepository = adminRepository;
        }

        public IEnumerable<ApplicationTechnology> GetTechnologies()
        {
            return _technologyRepository.GetTechnologies();
        }

        public IEnumerable<ApplicationTechnology> GetActiveTechnologies()
        {
            return _technologyRepository.GetActiveTechnologies();
        }

        public ApplicationTechnology GetById(int id)
        {
            return _technologyRepository.GetById(id);
        }

        public ApplicationTechnology GetByName(string name)
        {
            return _technologyRepository.GetByName(name);
        }

        public ApplicationTechnology Create(ApplicationTechnology technology, string user)
        {
            if (_technologyRepository.GetByName(technology.Name) != null)
                throw new InvalidOperationException($"A technology named '{technology.Name}' already exists.");

            technology.IsActive = true;
            technology.CreatedDate = DateTime.UtcNow;
            technology.ModifiedDate = DateTime.UtcNow;
            technology.CreatedBy = user;
            technology.ModifiedBy = user;

            var created = _technologyRepository.Add(technology);
            _technologyRepository.SaveChanges();

            _adminRepository.Log("Created", "ApplicationTechnology", created.TechnologyId, created.Name,
                $"Technology '{created.Name}' created", user);
            _adminRepository.SaveChanges();

            return created;
        }

        public ApplicationTechnology Update(ApplicationTechnology technology, string user)
        {
            var existing = _technologyRepository.GetById(technology.TechnologyId);
            if (existing == null)
                throw new InvalidOperationException($"Technology {technology.TechnologyId} not found.");

            var duplicate = _technologyRepository.GetByName(technology.Name);
            if (duplicate != null && duplicate.TechnologyId != technology.TechnologyId)
                throw new InvalidOperationException($"A technology named '{technology.Name}' already exists.");

            existing.Name = technology.Name;
            existing.Description = technology.Description;
            existing.IsActive = technology.IsActive;
            existing.SortOrder = technology.SortOrder;
            existing.ModifiedDate = DateTime.UtcNow;
            existing.ModifiedBy = user;

            _technologyRepository.SaveChanges();

            _adminRepository.Log("Updated", "ApplicationTechnology", existing.TechnologyId, existing.Name,
                $"Technology '{existing.Name}' updated", user);
            _adminRepository.SaveChanges();

            return existing;
        }

        public void ToggleActive(int technologyId, string user)
        {
            var technology = _technologyRepository.GetById(technologyId);
            if (technology == null)
                return;

            technology.IsActive = !technology.IsActive;
            technology.ModifiedDate = DateTime.UtcNow;
            technology.ModifiedBy = user;
            _technologyRepository.SaveChanges();

            _adminRepository.Log("Updated", "ApplicationTechnology", technologyId, technology.Name,
                $"Technology '{technology.Name}' {(technology.IsActive ? "activated" : "deactivated")}", user);
            _adminRepository.SaveChanges();
        }

        public void Delete(int technologyId, string user)
        {
            var technology = _technologyRepository.GetById(technologyId);
            if (technology == null)
                return;

            // Remove the join rows first so the FK is not violated
            // (the DB also has ON DELETE CASCADE as a backstop).
            var mappings = _technologyRepository.GetMappingsForTechnology(technologyId).ToList();
            foreach (var mapping in mappings)
                _technologyRepository.RemoveMapping(mapping);
            _technologyRepository.SaveChanges();

            _adminRepository.Log("Deleted", "ApplicationTechnology", technologyId, technology.Name,
                $"Technology '{technology.Name}' removed", user);
            _adminRepository.SaveChanges();

            _technologyRepository.Remove(technology);
            _technologyRepository.SaveChanges();
        }

        public IEnumerable<ApplicationTechnology> GetForApplication(int applicationId)
        {
            return _technologyRepository.GetForApplication(applicationId);
        }

        public void SetForApplication(int applicationId, IEnumerable<int> technologyIds, string user)
        {
            var ids = technologyIds == null
                ? Enumerable.Empty<int>()
                : technologyIds.Where(id => id > 0).Distinct();

            var current = _technologyRepository.GetMappingsForApplication(applicationId).ToList();
            var keep = new HashSet<int>(ids);

            foreach (var mapping in current.Where(m => !keep.Contains(m.TechnologyId)).ToList())
                _technologyRepository.RemoveMapping(mapping);

            var have = new HashSet<int>(current.Select(m => m.TechnologyId));
            foreach (var id in keep.Where(id => !have.Contains(id)))
                _technologyRepository.AddMapping(applicationId, id);

            if (current.Any(m => !keep.Contains(m.TechnologyId)) || keep.Any(id => !have.Contains(id)))
            {
                _technologyRepository.SaveChanges();

                _adminRepository.Log("Updated", "Application", applicationId, null,
                    $"Application technologies updated", user);
                _adminRepository.SaveChanges();
            }
        }
    }
}
