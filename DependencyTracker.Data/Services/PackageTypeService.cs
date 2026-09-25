using System;
using System.Collections.Generic;
using System.Linq;
using DependencyTracker.Data.Models;
using DependencyTracker.Data.Repositories;

namespace DependencyTracker.Data.Services
{
    public class PackageTypeService : IPackageTypeService
    {
        private readonly IPackageTypeRepository _packageTypeRepository;
        private readonly IPackageRepository _packageRepository;
        private readonly IAdminRepository _adminRepository;

        public PackageTypeService()
            : this(new PackageTypeRepository(), new PackageRepository(), new AdminRepository())
        {
        }

        public PackageTypeService(
            IPackageTypeRepository packageTypeRepository,
            IPackageRepository packageRepository,
            IAdminRepository adminRepository)
        {
            _packageTypeRepository = packageTypeRepository;
            _packageRepository = packageRepository;
            _adminRepository = adminRepository;
        }

        public IEnumerable<PackageType> GetPackageTypes()
        {
            return _packageTypeRepository.GetPackageTypes();
        }

        public IEnumerable<PackageType> GetActivePackageTypes()
        {
            return _packageTypeRepository.GetActivePackageTypes();
        }

        public PackageType GetById(int id)
        {
            return _packageTypeRepository.GetById(id);
        }

        public PackageType Create(PackageType packageType, string user)
        {
            if (_packageTypeRepository.GetByName(packageType.Name) != null)
                throw new InvalidOperationException($"A package type named '{packageType.Name}' already exists.");

            packageType.IsActive = true;
            packageType.CreatedDate = DateTime.UtcNow;
            packageType.ModifiedDate = DateTime.UtcNow;
            packageType.CreatedBy = user;
            packageType.ModifiedBy = user;

            var created = _packageTypeRepository.Add(packageType);
            _packageTypeRepository.SaveChanges();

            _adminRepository.Log("Created", "PackageType", created.PackageTypeId, created.Name,
                $"Package type '{created.Name}' created", user);
            _adminRepository.SaveChanges();

            return created;
        }

        public PackageType Update(PackageType packageType, string user)
        {
            var existing = _packageTypeRepository.GetById(packageType.PackageTypeId);
            if (existing == null)
                throw new InvalidOperationException($"Package type {packageType.PackageTypeId} not found.");

            var duplicate = _packageTypeRepository.GetByName(packageType.Name);
            if (duplicate != null && duplicate.PackageTypeId != packageType.PackageTypeId)
                throw new InvalidOperationException($"A package type named '{packageType.Name}' already exists.");

            existing.Name = packageType.Name;
            existing.Description = packageType.Description;
            existing.IsActive = packageType.IsActive;
            existing.SortOrder = packageType.SortOrder;
            existing.ModifiedDate = DateTime.UtcNow;
            existing.ModifiedBy = user;

            _packageTypeRepository.SaveChanges();

            _adminRepository.Log("Updated", "PackageType", existing.PackageTypeId, existing.Name,
                $"Package type '{existing.Name}' updated", user);
            _adminRepository.SaveChanges();

            return existing;
        }

        public void ToggleActive(int packageTypeId, string user)
        {
            var packageType = _packageTypeRepository.GetById(packageTypeId);
            if (packageType == null)
                return;

            packageType.IsActive = !packageType.IsActive;
            packageType.ModifiedDate = DateTime.UtcNow;
            packageType.ModifiedBy = user;
            _packageTypeRepository.SaveChanges();

            _adminRepository.Log("Updated", "PackageType", packageTypeId, packageType.Name,
                $"Package type '{packageType.Name}' {(packageType.IsActive ? "activated" : "deactivated")}", user);
            _adminRepository.SaveChanges();
        }

        public void Delete(int packageTypeId, string user)
        {
            var packageType = _packageTypeRepository.GetById(packageTypeId);
            if (packageType == null)
                return;

            // Packages referenced by applications are removed along with their
            // mappings first so the foreign keys are not violated (the DB also
            // has ON DELETE CASCADE as a backstop).
            var packages = _packageRepository.Query()
                .Where(p => p.PackageTypeId == packageTypeId)
                .ToList();
            foreach (var package in packages)
            {
                var mappings = _packageRepository.GetMappingsForPackage(package.PackageId).ToList();
                foreach (var mapping in mappings)
                    _packageRepository.RemoveMapping(mapping);
                _packageRepository.SaveChanges();
                _packageRepository.Remove(package);
            }
            _packageRepository.SaveChanges();

            _adminRepository.Log("Deleted", "PackageType", packageTypeId, packageType.Name,
                $"Package type '{packageType.Name}' removed", user);
            _adminRepository.SaveChanges();

            _packageTypeRepository.Remove(packageType);
            _packageTypeRepository.SaveChanges();
        }
    }
}