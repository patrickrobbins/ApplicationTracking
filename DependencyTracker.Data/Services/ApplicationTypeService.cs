using System;
using System.Collections.Generic;
using System.Linq;
using DependencyTracker.Data.Models;
using DependencyTracker.Data.Repositories;

namespace DependencyTracker.Data.Services
{
    public class ApplicationTypeService : IApplicationTypeService
    {
        private readonly IApplicationTypeRepository _applicationTypeRepository;
        private readonly IApplicationRepository _applicationRepository;
        private readonly IAdminRepository _adminRepository;

        public ApplicationTypeService()
            : this(new ApplicationTypeRepository(), new ApplicationRepository(), new AdminRepository())
        {
        }

        public ApplicationTypeService(
            IApplicationTypeRepository applicationTypeRepository,
            IApplicationRepository applicationRepository,
            IAdminRepository adminRepository)
        {
            _applicationTypeRepository = applicationTypeRepository;
            _applicationRepository = applicationRepository;
            _adminRepository = adminRepository;
        }

        public IEnumerable<ApplicationType> GetApplicationTypes()
        {
            return _applicationTypeRepository.GetApplicationTypes();
        }

        public IEnumerable<ApplicationType> GetActiveApplicationTypes()
        {
            return _applicationTypeRepository.GetActiveApplicationTypes();
        }

        public ApplicationType GetById(int id)
        {
            return _applicationTypeRepository.GetById(id);
        }

        public ApplicationType Create(ApplicationType applicationType, string user)
        {
            if (_applicationTypeRepository.GetByName(applicationType.Name) != null)
                throw new InvalidOperationException($"An application type named '{applicationType.Name}' already exists.");

            applicationType.IsActive = true;
            applicationType.CreatedDate = DateTime.UtcNow;
            applicationType.ModifiedDate = DateTime.UtcNow;
            applicationType.CreatedBy = user;
            applicationType.ModifiedBy = user;

            var created = _applicationTypeRepository.Add(applicationType);
            _applicationTypeRepository.SaveChanges();

            _adminRepository.Log("Created", "ApplicationType", created.ApplicationTypeId, created.Name,
                $"Application type '{created.Name}' created", user);
            _adminRepository.SaveChanges();

            return created;
        }

        public ApplicationType Update(ApplicationType applicationType, string user)
        {
            var existing = _applicationTypeRepository.GetById(applicationType.ApplicationTypeId);
            if (existing == null)
                throw new InvalidOperationException($"Application type {applicationType.ApplicationTypeId} not found.");

            var duplicate = _applicationTypeRepository.GetByName(applicationType.Name);
            if (duplicate != null && duplicate.ApplicationTypeId != applicationType.ApplicationTypeId)
                throw new InvalidOperationException($"An application type named '{applicationType.Name}' already exists.");

            existing.Name = applicationType.Name;
            existing.Description = applicationType.Description;
            existing.IsActive = applicationType.IsActive;
            existing.SortOrder = applicationType.SortOrder;
            existing.ModifiedDate = DateTime.UtcNow;
            existing.ModifiedBy = user;

            _applicationTypeRepository.SaveChanges();

            _adminRepository.Log("Updated", "ApplicationType", existing.ApplicationTypeId, existing.Name,
                $"Application type '{existing.Name}' updated", user);
            _adminRepository.SaveChanges();

            return existing;
        }

        public void ToggleActive(int applicationTypeId, string user)
        {
            var applicationType = _applicationTypeRepository.GetById(applicationTypeId);
            if (applicationType == null)
                return;

            applicationType.IsActive = !applicationType.IsActive;
            applicationType.ModifiedDate = DateTime.UtcNow;
            applicationType.ModifiedBy = user;
            _applicationTypeRepository.SaveChanges();

            _adminRepository.Log("Updated", "ApplicationType", applicationTypeId, applicationType.Name,
                $"Application type '{applicationType.Name}' {(applicationType.IsActive ? "activated" : "deactivated")}", user);
            _adminRepository.SaveChanges();
        }

        public void Delete(int applicationTypeId, string user)
        {
            var applicationType = _applicationTypeRepository.GetById(applicationTypeId);
            if (applicationType == null)
                return;

            // Detach applications from the type first so the FK is not
            // violated (the DB also has ON DELETE SET NULL as a backstop).
            var applications = _applicationRepository.Query()
                .Where(a => a.ApplicationTypeId == applicationTypeId)
                .ToList();
            foreach (var application in applications)
            {
                application.ApplicationTypeId = null;
                application.ModifiedDate = DateTime.UtcNow;
                application.ModifiedBy = user;
            }
            _applicationRepository.SaveChanges();

            _adminRepository.Log("Deleted", "ApplicationType", applicationTypeId, applicationType.Name,
                $"Application type '{applicationType.Name}' removed", user);
            _adminRepository.SaveChanges();

            _applicationTypeRepository.Remove(applicationType);
            _applicationTypeRepository.SaveChanges();
        }
    }
}