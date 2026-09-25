using System;
using System.Collections.Generic;
using System.Linq;
using DependencyTracker.Data.Models;
using DependencyTracker.Data.Repositories;

namespace DependencyTracker.Data.Services
{
    public class PackageService : IPackageService
    {
        private readonly IPackageRepository _packageRepository;
        private readonly IPackageTypeRepository _packageTypeRepository;
        private readonly IAdminRepository _adminRepository;

        public PackageService()
            : this(new PackageRepository(), new PackageTypeRepository(), new AdminRepository())
        {
        }

        public PackageService(
            IPackageRepository packageRepository,
            IPackageTypeRepository packageTypeRepository,
            IAdminRepository adminRepository)
        {
            _packageRepository = packageRepository;
            _packageTypeRepository = packageTypeRepository;
            _adminRepository = adminRepository;
        }

        public Package GetById(int id)
        {
            return _packageRepository.GetById(id);
        }

        public Package GetByTypeAndName(int packageTypeId, string name)
        {
            return _packageRepository.GetByTypeAndName(packageTypeId, name);
        }

        public Package GetOrCreateByTypeAndName(int packageTypeId, string name, string user)
        {
            var trimmed = string.IsNullOrWhiteSpace(name) ? null : name.Trim();
            if (trimmed == null)
                return null;

            var existing = _packageRepository.GetByTypeAndName(packageTypeId, trimmed);
            if (existing != null)
                return existing;

            if (_packageTypeRepository.GetById(packageTypeId) == null)
                throw new InvalidOperationException($"Package type {packageTypeId} not found.");

            return Create(new Package { PackageTypeId = packageTypeId, Name = trimmed }, user);
        }

        public Package Create(Package package, string user)
        {
            if (_packageRepository.GetByTypeAndName(package.PackageTypeId, package.Name) != null)
                throw new InvalidOperationException($"A package named '{package.Name}' already exists for this type.");

            package.IsActive = true;
            package.CreatedDate = DateTime.UtcNow;
            package.ModifiedDate = DateTime.UtcNow;
            package.CreatedBy = user;
            package.ModifiedBy = user;

            var created = _packageRepository.Add(package);
            _packageRepository.SaveChanges();

            _adminRepository.Log("Created", "Package", created.PackageId, created.Name,
                $"Package '{created.Name}' created", user);
            _adminRepository.SaveChanges();

            return created;
        }

        public void Delete(int packageId, string user)
        {
            var package = _packageRepository.GetById(packageId);
            if (package == null)
                return;

            // Remove the join rows first so the FK is not violated
            // (the DB also has ON DELETE CASCADE as a backstop).
            var mappings = _packageRepository.GetMappingsForPackage(packageId).ToList();
            foreach (var mapping in mappings)
                _packageRepository.RemoveMapping(mapping);
            _packageRepository.SaveChanges();

            _adminRepository.Log("Deleted", "Package", packageId, package.Name,
                $"Package '{package.Name}' removed", user);
            _adminRepository.SaveChanges();

            _packageRepository.Remove(package);
            _packageRepository.SaveChanges();
        }

        public IEnumerable<ApplicationPackageMapping> GetMappingsForApplication(int applicationId)
        {
            return _packageRepository.GetMappingsForApplication(applicationId);
        }

        public void SetForApplication(int applicationId, IEnumerable<ApplicationPackageInput> inputs, string user)
        {
            var valid = new List<ApplicationPackageInput>();
            if (inputs != null)
            {
                foreach (var input in inputs)
                {
                    var name = string.IsNullOrWhiteSpace(input.Name) ? null : input.Name.Trim();
                    if (name == null || input.PackageTypeId <= 0)
                        continue;
                    valid.Add(new ApplicationPackageInput
                    {
                        PackageTypeId = input.PackageTypeId,
                        Name = name,
                        Version = string.IsNullOrWhiteSpace(input.Version) ? null : input.Version.Trim()
                    });
                }
            }

            // De-duplicate on (type, name) - the last row wins for the version.
            var selected = new List<ApplicationPackageInput>();
            foreach (var input in valid)
            {
                selected.RemoveAll(s => s.DedupeKey == input.DedupeKey);
                selected.Add(input);
            }

            var current = _packageRepository.GetMappingsForApplication(applicationId).ToList();
            var keepPackageIds = new HashSet<int>();

            var changed = false;
            foreach (var input in selected)
            {
                var package = GetOrCreateByTypeAndName(input.PackageTypeId, input.Name, user);
                if (package == null)
                    continue;

                keepPackageIds.Add(package.PackageId);

                var mapping = _packageRepository.GetMapping(applicationId, package.PackageId);
                if (mapping == null)
                {
                    _packageRepository.AddMapping(applicationId, package.PackageId, input.Version);
                    changed = true;
                }
                else if (mapping.Version != input.Version)
                {
                    mapping.Version = input.Version;
                    changed = true;
                }
            }

            foreach (var mapping in current.Where(m => !keepPackageIds.Contains(m.PackageId)).ToList())
            {
                _packageRepository.RemoveMapping(mapping);
                changed = true;
            }

            if (changed)
            {
                _packageRepository.SaveChanges();

                _adminRepository.Log("Updated", "Application", applicationId, null,
                    $"Application packages updated", user);
                _adminRepository.SaveChanges();
            }
        }
    }
}