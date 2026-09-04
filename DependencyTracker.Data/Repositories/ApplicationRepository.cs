using System.Linq;
using System.Collections.Generic;
using DependencyTracker.Data.Models;

namespace DependencyTracker.Data.Repositories
{
    public class ApplicationRepository : Repository<Application>, IApplicationRepository
    {
        public ApplicationRepository() : base() { }

        public ApplicationRepository(DependencyTrackerDbContext context) : base(context) { }

        /// <summary>All applications, excluding soft-deleted ones.</summary>
        public override IEnumerable<Application> GetAll()
        {
            return DbSet.Where(a => !a.IsDeleted).ToList();
        }

        public IEnumerable<Application> GetAllIncludingDeleted()
        {
            return DbSet.ToList();
        }

        public IEnumerable<Application> GetVersionsByName(string name)
        {
            return DbSet
                .Where(a => a.Name == name && !a.IsDeleted)
                .OrderBy(a => a.Version)
                .ToList();
        }

        public IEnumerable<Application> Search(string term, string environment, string status, string criticality, int? categoryId, string version, int? technologyId, bool includeDeleted)
        {
            var query = DbSet.Where(a => includeDeleted || !a.IsDeleted);

            if (!string.IsNullOrWhiteSpace(term))
            {
                query = query.Where(a =>
                    a.Name.Contains(term) ||
                    a.Description.Contains(term) ||
                    a.BusinessGroup.Contains(term) ||
                    a.BusinessOwner.Contains(term) ||
                    a.BusinessOwnerEmail.Contains(term) ||
                    a.BusinessBackup.Contains(term) ||
                    a.BusinessBackupEmail.Contains(term) ||
                    a.TechnicalOwner.Contains(term) ||
                    a.TechnicalOwnerEmail.Contains(term));
            }

            if (!string.IsNullOrWhiteSpace(environment))
                query = query.Where(a => a.Environment == environment);

            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(a => a.Status == status);

            if (!string.IsNullOrWhiteSpace(criticality))
                query = query.Where(a => a.CriticalityLevel == criticality);

            if (categoryId.HasValue)
                query = query.Where(a => a.CategoryId == categoryId.Value);

            if (technologyId.HasValue)
                query = query.Where(a => a.TechnologyMappings.Any(m => m.TechnologyId == technologyId.Value));

            if (!string.IsNullOrWhiteSpace(version))
                query = query.Where(a => a.Version == version);

            return query
                .OrderBy(a => a.Name)
                .ThenBy(a => a.Version)
                .ToList();
        }

        public Application GetByName(string name)
        {
            return DbSet.FirstOrDefault(a => a.Name == name && !a.IsDeleted);
        }

        public IEnumerable<string> GetAllEnvironments()
        {
            return DbSet.Where(a => !a.IsDeleted)
                .Select(a => a.Environment)
                .Distinct()
                .OrderBy(e => e)
                .ToList();
        }

        public IEnumerable<string> GetAllStatuses()
        {
            return DbSet.Where(a => !a.IsDeleted)
                .Select(a => a.Status)
                .Distinct()
                .OrderBy(s => s)
                .ToList();
        }

        public IEnumerable<string> GetAllCriticalities()
        {
            return DbSet.Where(a => !a.IsDeleted)
                .Select(a => a.CriticalityLevel)
                .Distinct()
                .OrderBy(c => c)
                .ToList();
        }

        public IEnumerable<string> GetAllVersions()
        {
            return DbSet.Where(a => !a.IsDeleted)
                .Select(a => a.Version)
                .Where(v => v != null && v != "")
                .Distinct()
                .OrderBy(v => v)
                .ToList();
        }
    }
}
