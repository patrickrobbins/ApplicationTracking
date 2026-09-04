using System;
using System.Collections.Generic;
using System.Linq;
using DependencyTracker.Data.Models;
using DependencyTracker.Data.Repositories;

namespace DependencyTracker.Data.Services
{
    public class ApplicationFamilyService : IApplicationFamilyService
    {
        private readonly IApplicationFamilyRepository _familyRepository;
        private readonly IApplicationRepository _applicationRepository;
        private readonly IAdminRepository _adminRepository;

        public ApplicationFamilyService()
            : this(new ApplicationFamilyRepository(), new ApplicationRepository(), new AdminRepository())
        {
        }

        public ApplicationFamilyService(
            IApplicationFamilyRepository familyRepository,
            IApplicationRepository applicationRepository,
            IAdminRepository adminRepository)
        {
            _familyRepository = familyRepository;
            _applicationRepository = applicationRepository;
            _adminRepository = adminRepository;
        }

        public IEnumerable<ApplicationFamily> GetFamilies()
        {
            return _familyRepository.GetFamilies();
        }

        public IEnumerable<ApplicationFamily> GetActiveFamilies()
        {
            return _familyRepository.GetActiveFamilies();
        }

        public ApplicationFamily GetById(int id)
        {
            return _familyRepository.GetById(id);
        }

        public ApplicationFamily Create(ApplicationFamily family, string user)
        {
            if (_familyRepository.GetByName(family.Name) != null)
                throw new InvalidOperationException($"A family named '{family.Name}' already exists.");

            family.IsActive = true;
            family.CreatedDate = DateTime.UtcNow;
            family.ModifiedDate = DateTime.UtcNow;
            family.CreatedBy = user;
            family.ModifiedBy = user;

            var created = _familyRepository.Add(family);
            _familyRepository.SaveChanges();

            _adminRepository.Log("Created", "ApplicationFamily", created.FamilyId, created.Name,
                $"Family '{created.Name}' created", user);
            _adminRepository.SaveChanges();

            return created;
        }

        public ApplicationFamily Update(ApplicationFamily family, string user)
        {
            var existing = _familyRepository.GetById(family.FamilyId);
            if (existing == null)
                throw new InvalidOperationException($"Family {family.FamilyId} not found.");

            var duplicate = _familyRepository.GetByName(family.Name);
            if (duplicate != null && duplicate.FamilyId != family.FamilyId)
                throw new InvalidOperationException($"A family named '{family.Name}' already exists.");

            existing.Name = family.Name;
            existing.Description = family.Description;
            existing.IsActive = family.IsActive;
            existing.SortOrder = family.SortOrder;
            existing.ModifiedDate = DateTime.UtcNow;
            existing.ModifiedBy = user;

            _familyRepository.SaveChanges();

            _adminRepository.Log("Updated", "ApplicationFamily", existing.FamilyId, existing.Name,
                $"Family '{existing.Name}' updated", user);
            _adminRepository.SaveChanges();

            return existing;
        }

        public void ToggleActive(int familyId, string user)
        {
            var family = _familyRepository.GetById(familyId);
            if (family == null)
                return;

            family.IsActive = !family.IsActive;
            family.ModifiedDate = DateTime.UtcNow;
            family.ModifiedBy = user;
            _familyRepository.SaveChanges();

            _adminRepository.Log("Updated", "ApplicationFamily", familyId, family.Name,
                $"Family '{family.Name}' {(family.IsActive ? "activated" : "deactivated")}", user);
            _adminRepository.SaveChanges();
        }

        public void Delete(int familyId, string user)
        {
            var family = _familyRepository.GetById(familyId);
            if (family == null)
                return;

            // Detach applications from the family first so the FK is not
            // violated (the DB also has ON DELETE SET NULL as a backstop).
            var applications = _applicationRepository.Query()
                .Where(a => a.FamilyId == familyId)
                .ToList();
            foreach (var application in applications)
            {
                application.FamilyId = null;
                application.ModifiedDate = DateTime.UtcNow;
                application.ModifiedBy = user;
            }
            _applicationRepository.SaveChanges();

            _adminRepository.Log("Deleted", "ApplicationFamily", familyId, family.Name,
                $"Family '{family.Name}' removed", user);
            _adminRepository.SaveChanges();

            _familyRepository.Remove(family);
            _familyRepository.SaveChanges();
        }
    }
}