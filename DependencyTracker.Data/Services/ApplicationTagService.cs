using System;
using System.Collections.Generic;
using System.Linq;
using DependencyTracker.Data.Models;
using DependencyTracker.Data.Repositories;

namespace DependencyTracker.Data.Services
{
    public class ApplicationTagService : IApplicationTagService
    {
        private readonly IApplicationTagRepository _tagRepository;
        private readonly IAdminRepository _adminRepository;

        public ApplicationTagService()
            : this(new ApplicationTagRepository(), new AdminRepository())
        {
        }

        public ApplicationTagService(
            IApplicationTagRepository tagRepository,
            IAdminRepository adminRepository)
        {
            _tagRepository = tagRepository;
            _adminRepository = adminRepository;
        }

        public IEnumerable<ApplicationTag> GetTags()
        {
            return _tagRepository.GetTags();
        }

        public IEnumerable<ApplicationTag> GetActiveTags()
        {
            return _tagRepository.GetActiveTags();
        }

        public ApplicationTag GetById(int id)
        {
            return _tagRepository.GetById(id);
        }

        public ApplicationTag GetByName(string name)
        {
            return _tagRepository.GetByName(name);
        }

        public ApplicationTag GetOrCreateByName(string name, string user)
        {
            var trimmed = string.IsNullOrWhiteSpace(name) ? null : name.Trim();
            if (trimmed == null)
                return null;

            var existing = _tagRepository.GetByName(trimmed);
            if (existing != null)
                return existing;

            return Create(new ApplicationTag { Name = trimmed }, user);
        }

        public ApplicationTag Create(ApplicationTag tag, string user)
        {
            if (_tagRepository.GetByName(tag.Name) != null)
                throw new InvalidOperationException($"A tag named '{tag.Name}' already exists.");

            tag.IsActive = true;
            tag.CreatedDate = DateTime.UtcNow;
            tag.ModifiedDate = DateTime.UtcNow;
            tag.CreatedBy = user;
            tag.ModifiedBy = user;

            var created = _tagRepository.Add(tag);
            _tagRepository.SaveChanges();

            _adminRepository.Log("Created", "ApplicationTag", created.TagId, created.Name,
                $"Tag '{created.Name}' created", user);
            _adminRepository.SaveChanges();

            return created;
        }

        public ApplicationTag Update(ApplicationTag tag, string user)
        {
            var existing = _tagRepository.GetById(tag.TagId);
            if (existing == null)
                throw new InvalidOperationException($"Tag {tag.TagId} not found.");

            var duplicate = _tagRepository.GetByName(tag.Name);
            if (duplicate != null && duplicate.TagId != tag.TagId)
                throw new InvalidOperationException($"A tag named '{tag.Name}' already exists.");

            existing.Name = tag.Name;
            existing.Description = tag.Description;
            existing.IsActive = tag.IsActive;
            existing.SortOrder = tag.SortOrder;
            existing.ModifiedDate = DateTime.UtcNow;
            existing.ModifiedBy = user;

            _tagRepository.SaveChanges();

            _adminRepository.Log("Updated", "ApplicationTag", existing.TagId, existing.Name,
                $"Tag '{existing.Name}' updated", user);
            _adminRepository.SaveChanges();

            return existing;
        }

        public void ToggleActive(int tagId, string user)
        {
            var tag = _tagRepository.GetById(tagId);
            if (tag == null)
                return;

            tag.IsActive = !tag.IsActive;
            tag.ModifiedDate = DateTime.UtcNow;
            tag.ModifiedBy = user;
            _tagRepository.SaveChanges();

            _adminRepository.Log("Updated", "ApplicationTag", tagId, tag.Name,
                $"Tag '{tag.Name}' {(tag.IsActive ? "activated" : "deactivated")}", user);
            _adminRepository.SaveChanges();
        }

        public void Delete(int tagId, string user)
        {
            var tag = _tagRepository.GetById(tagId);
            if (tag == null)
                return;

            // Remove the join rows first so the FK is not violated
            // (the DB also has ON DELETE CASCADE as a backstop).
            var mappings = _tagRepository.GetMappingsForTag(tagId).ToList();
            foreach (var mapping in mappings)
                _tagRepository.RemoveMapping(mapping);
            _tagRepository.SaveChanges();

            _adminRepository.Log("Deleted", "ApplicationTag", tagId, tag.Name,
                $"Tag '{tag.Name}' removed", user);
            _adminRepository.SaveChanges();

            _tagRepository.Remove(tag);
            _tagRepository.SaveChanges();
        }

        public IEnumerable<ApplicationTag> GetForApplication(int applicationId)
        {
            return _tagRepository.GetForApplication(applicationId);
        }

        public void SetForApplication(int applicationId, IEnumerable<int> tagIds, string user)
        {
            var ids = tagIds == null
                ? Enumerable.Empty<int>()
                : tagIds.Where(id => id > 0).Distinct();

            var current = _tagRepository.GetMappingsForApplication(applicationId).ToList();
            var keep = new HashSet<int>(ids);

            foreach (var mapping in current.Where(m => !keep.Contains(m.TagId)).ToList())
                _tagRepository.RemoveMapping(mapping);

            var have = new HashSet<int>(current.Select(m => m.TagId));
            foreach (var id in keep.Where(id => !have.Contains(id)))
                _tagRepository.AddMapping(applicationId, id);

            if (current.Any(m => !keep.Contains(m.TagId)) || keep.Any(id => !have.Contains(id)))
            {
                _tagRepository.SaveChanges();

                _adminRepository.Log("Updated", "Application", applicationId, null,
                    $"Application tags updated", user);
                _adminRepository.SaveChanges();
            }
        }
    }
}