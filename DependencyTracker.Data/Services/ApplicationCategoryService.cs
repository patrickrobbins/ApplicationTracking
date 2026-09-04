using System;
using System.Collections.Generic;
using System.Linq;
using DependencyTracker.Data.Models;
using DependencyTracker.Data.Repositories;

namespace DependencyTracker.Data.Services
{
    public class ApplicationCategoryService : IApplicationCategoryService
    {
        private readonly IApplicationCategoryRepository _categoryRepository;
        private readonly IApplicationRepository _applicationRepository;
        private readonly IAdminRepository _adminRepository;

        public ApplicationCategoryService()
            : this(new ApplicationCategoryRepository(), new ApplicationRepository(), new AdminRepository())
        {
        }

        public ApplicationCategoryService(
            IApplicationCategoryRepository categoryRepository,
            IApplicationRepository applicationRepository,
            IAdminRepository adminRepository)
        {
            _categoryRepository = categoryRepository;
            _applicationRepository = applicationRepository;
            _adminRepository = adminRepository;
        }

        public IEnumerable<ApplicationCategory> GetCategories()
        {
            return _categoryRepository.GetCategories();
        }

        public IEnumerable<ApplicationCategory> GetActiveCategories()
        {
            return _categoryRepository.GetActiveCategories();
        }

        public ApplicationCategory GetById(int id)
        {
            return _categoryRepository.GetById(id);
        }

        public ApplicationCategory Create(ApplicationCategory category, string user)
        {
            if (_categoryRepository.GetByName(category.Name) != null)
                throw new InvalidOperationException($"A category named '{category.Name}' already exists.");

            category.IsActive = true;
            category.CreatedDate = DateTime.UtcNow;
            category.ModifiedDate = DateTime.UtcNow;
            category.CreatedBy = user;
            category.ModifiedBy = user;

            var created = _categoryRepository.Add(category);
            _categoryRepository.SaveChanges();

            _adminRepository.Log("Created", "ApplicationCategory", created.CategoryId, created.Name,
                $"Category '{created.Name}' created", user);
            _adminRepository.SaveChanges();

            return created;
        }

        public ApplicationCategory Update(ApplicationCategory category, string user)
        {
            var existing = _categoryRepository.GetById(category.CategoryId);
            if (existing == null)
                throw new InvalidOperationException($"Category {category.CategoryId} not found.");

            var duplicate = _categoryRepository.GetByName(category.Name);
            if (duplicate != null && duplicate.CategoryId != category.CategoryId)
                throw new InvalidOperationException($"A category named '{category.Name}' already exists.");

            existing.Name = category.Name;
            existing.Description = category.Description;
            existing.IsActive = category.IsActive;
            existing.SortOrder = category.SortOrder;
            existing.ModifiedDate = DateTime.UtcNow;
            existing.ModifiedBy = user;

            _categoryRepository.SaveChanges();

            _adminRepository.Log("Updated", "ApplicationCategory", existing.CategoryId, existing.Name,
                $"Category '{existing.Name}' updated", user);
            _adminRepository.SaveChanges();

            return existing;
        }

        public void ToggleActive(int categoryId, string user)
        {
            var category = _categoryRepository.GetById(categoryId);
            if (category == null)
                return;

            category.IsActive = !category.IsActive;
            category.ModifiedDate = DateTime.UtcNow;
            category.ModifiedBy = user;
            _categoryRepository.SaveChanges();

            _adminRepository.Log("Updated", "ApplicationCategory", categoryId, category.Name,
                $"Category '{category.Name}' {(category.IsActive ? "activated" : "deactivated")}", user);
            _adminRepository.SaveChanges();
        }

        public void Delete(int categoryId, string user)
        {
            var category = _categoryRepository.GetById(categoryId);
            if (category == null)
                return;

            // Detach applications from the category first so the FK is not
            // violated (the DB also has ON DELETE SET NULL as a backstop).
            var applications = _applicationRepository.Query()
                .Where(a => a.CategoryId == categoryId)
                .ToList();
            foreach (var application in applications)
            {
                application.CategoryId = null;
                application.ModifiedDate = DateTime.UtcNow;
                application.ModifiedBy = user;
            }
            _applicationRepository.SaveChanges();

            _adminRepository.Log("Deleted", "ApplicationCategory", categoryId, category.Name,
                $"Category '{category.Name}' removed", user);
            _adminRepository.SaveChanges();

            _categoryRepository.Remove(category);
            _categoryRepository.SaveChanges();
        }
    }
}
