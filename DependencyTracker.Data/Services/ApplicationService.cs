using System;
using System.Collections.Generic;
using System.Linq;
using DependencyTracker.Data.Models;
using DependencyTracker.Data.Repositories;

namespace DependencyTracker.Data.Services
{
    public class ApplicationService : IApplicationService
    {
        private readonly IApplicationRepository _applicationRepository;
        private readonly IAdminRepository _adminRepository;
        private readonly IDependencyRepository _dependencyRepository;

        public ApplicationService()
            : this(new ApplicationRepository(), new AdminRepository(), new DependencyRepository())
        {
        }

        public ApplicationService(
            IApplicationRepository applicationRepository,
            IAdminRepository adminRepository,
            IDependencyRepository dependencyRepository)
        {
            _applicationRepository = applicationRepository;
            _adminRepository = adminRepository;
            _dependencyRepository = dependencyRepository;
        }

        public Application GetById(int id)
        {
            return _applicationRepository.GetById(id);
        }

        public IEnumerable<Application> GetAll()
        {
            return _applicationRepository.GetAll();
        }

        public IEnumerable<Application> GetAllIncludingDeleted()
        {
            return _applicationRepository.GetAllIncludingDeleted();
        }

        public IEnumerable<Application> GetVersionsByName(string name)
        {
            return _applicationRepository.GetVersionsByName(name);
        }

        public IEnumerable<Application> Search(string term, string environment, string status, string criticality, int? categoryId, string version, int? technologyId, bool includeDeleted)
        {
            return _applicationRepository.Search(term, environment, status, criticality, categoryId, version, technologyId, includeDeleted);
        }

        public Application Create(Application application, string user)
        {
            application.Status = string.IsNullOrWhiteSpace(application.Status) ? "Active" : application.Status;
            application.Version = string.IsNullOrWhiteSpace(application.Version) ? null : application.Version.Trim();
            application.CreatedDate = DateTime.UtcNow;
            application.ModifiedDate = DateTime.UtcNow;
            application.CreatedBy = user;
            application.ModifiedBy = user;

            var created = _applicationRepository.Add(application);
            _applicationRepository.SaveChanges();

            _adminRepository.Log("Created", "Application", created.ApplicationId, created.Name,
                $"Application '{created.Name}' created", user);
            _adminRepository.SaveChanges();

            return created;
        }

        public Application Update(Application application, string user)
        {
            var existing = _applicationRepository.GetById(application.ApplicationId);
            if (existing == null)
                throw new InvalidOperationException($"Application {application.ApplicationId} not found.");

            existing.Name = application.Name;
            existing.Version = string.IsNullOrWhiteSpace(application.Version) ? null : application.Version.Trim();
            existing.Description = application.Description;
            existing.BusinessGroup = application.BusinessGroup;
            existing.BusinessOwner = application.BusinessOwner;
            existing.BusinessOwnerEmail = application.BusinessOwnerEmail;
            existing.BusinessBackup = application.BusinessBackup;
            existing.BusinessBackupEmail = application.BusinessBackupEmail;
            existing.TechnicalOwner = application.TechnicalOwner;
            existing.TechnicalOwnerEmail = application.TechnicalOwnerEmail;
            existing.CategoryId = application.CategoryId;
            existing.Environment = application.Environment;
            existing.CriticalityLevel = application.CriticalityLevel;
            existing.DefaultDependencyCriticality = application.DefaultDependencyCriticality;
            existing.DefaultDependencyImpact = application.DefaultDependencyImpact;
            existing.Status = application.Status;
            existing.ExternalUrl = application.ExternalUrl;
            existing.SourcePath = application.SourcePath;
            existing.ApplicationIDE = application.ApplicationIDE;
            existing.FrameworkVersion = application.FrameworkVersion;
            existing.DocumentationLink = application.DocumentationLink;
            existing.SourceControlLocation = application.SourceControlLocation;
            existing.HoursOfOperation = application.HoursOfOperation;
            existing.MaintenanceWindow = application.MaintenanceWindow;
            existing.MaintenanceNotificationEmail = application.MaintenanceNotificationEmail;
            existing.ApplicationSummary = application.ApplicationSummary;
            existing.TriageSteps = application.TriageSteps;
            existing.ModifiedDate = DateTime.UtcNow;
            existing.ModifiedBy = user;

            _applicationRepository.SaveChanges();

            _adminRepository.Log("Updated", "Application", existing.ApplicationId, existing.Name,
                $"Application '{existing.Name}' updated", user);
            _adminRepository.SaveChanges();

            return existing;
        }

        public void SoftDelete(int applicationId, string user)
        {
            var application = _applicationRepository.GetById(applicationId);
            if (application == null)
                return;

            application.IsDeleted = true;
            application.ModifiedDate = DateTime.UtcNow;
            application.ModifiedBy = user;
            _applicationRepository.SaveChanges();

            _adminRepository.Log("Deleted", "Application", applicationId, application.Name,
                $"Application '{application.Name}' soft-deleted", user);
            _adminRepository.SaveChanges();
        }

        public void HardDelete(int applicationId, string user)
        {
            var application = _applicationRepository.GetById(applicationId);
            if (application == null)
                return;

            _adminRepository.Log("Deleted", "Application", applicationId, application.Name,
                $"Application '{application.Name}' permanently deleted", user);
            _adminRepository.SaveChanges();

            _applicationRepository.Remove(application);
            _applicationRepository.SaveChanges();
        }

        public Application Restore(int applicationId, string user)
        {
            var application = _applicationRepository.GetById(applicationId);
            if (application == null)
                return null;

            application.IsDeleted = false;
            application.ModifiedDate = DateTime.UtcNow;
            application.ModifiedBy = user;
            _applicationRepository.SaveChanges();

            _adminRepository.Log("Restored", "Application", applicationId, application.Name,
                $"Application '{application.Name}' restored", user);
            _adminRepository.SaveChanges();

            return application;
        }

        public bool CanDelete(int applicationId)
        {
            return _dependencyRepository.CountForApplication(applicationId) == 0;
        }

        public IEnumerable<string> GetAllEnvironments()
        {
            return _applicationRepository.GetAllEnvironments();
        }

        public IEnumerable<string> GetAllStatuses()
        {
            return _applicationRepository.GetAllStatuses();
        }

        public IEnumerable<string> GetAllCriticalities()
        {
            return _applicationRepository.GetAllCriticalities();
        }

        public IEnumerable<string> GetAllVersions()
        {
            return _applicationRepository.GetAllVersions();
        }

        public int TotalCount()
        {
            return _applicationRepository.Query().Count(a => !a.IsDeleted);
        }
    }
}
