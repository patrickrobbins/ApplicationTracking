using System;
using System.Collections.Generic;
using System.Linq;
using DependencyTracker.Data.Models;
using DependencyTracker.Data.Repositories;

namespace DependencyTracker.Data.Services
{
    public class ApplicationDllService : IApplicationDllService
    {
        private readonly IApplicationDllRepository _repository;
        private readonly IAdminRepository _adminRepository;

        public ApplicationDllService(IApplicationDllRepository repository, IAdminRepository adminRepository)
        {
            _repository = repository;
            _adminRepository = adminRepository;
        }

        public IEnumerable<ApplicationDll> GetForApplication(int applicationId)
        {
            return _repository.GetForApplication(applicationId);
        }

        public int CountForApplication(int applicationId)
        {
            return _repository.CountForApplication(applicationId);
        }

        public IEnumerable<ApplicationDll> FindByDll(string dllName, string dllOperator, string version)
        {
            return _repository.FindByDll(dllName, dllOperator, version);
        }

        public int SyncFromDiscovery(int applicationId, string applicationName, IEnumerable<DiscoveredDll> dlls, string user)
        {
            var existing = _repository.GetForApplication(applicationId).ToList();
            foreach (var dll in existing)
                _repository.Remove(dll);
            _repository.SaveChanges();

            var count = 0;
            foreach (var group in (dlls ?? Enumerable.Empty<DiscoveredDll>())
                .Where(d => d != null && !string.IsNullOrWhiteSpace(d.FileName))
                .GroupBy(d => d.FileName, StringComparer.OrdinalIgnoreCase))
            {
                var first = group.First();
                _repository.Add(new ApplicationDll
                {
                    ApplicationId = applicationId,
                    FileName = first.FileName,
                    Version = first.Version,
                    RelativePath = first.RelativePath,
                    ModifiedDate = DateTime.UtcNow,
                    ModifiedBy = user
                });
                count++;
            }
            _repository.SaveChanges();

            _adminRepository.Log("Updated", "Application", applicationId, applicationName,
                count + " DLL(s) synced from source path", user);
            _adminRepository.SaveChanges();

            return count;
        }
    }
}
