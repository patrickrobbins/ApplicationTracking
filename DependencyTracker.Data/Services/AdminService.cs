using System;
using System.Collections.Generic;
using DependencyTracker.Data.Models;
using DependencyTracker.Data.Repositories;

namespace DependencyTracker.Data.Services
{
    public class AdminService : IAdminService
    {
        private readonly IAdminRepository _adminRepository;

        public AdminService() : this(new AdminRepository())
        {
        }

        public AdminService(IAdminRepository adminRepository)
        {
            _adminRepository = adminRepository;
        }

        public IEnumerable<AdGroup> GetActiveGroups()
        {
            return _adminRepository.GetActiveGroups();
        }

        public IEnumerable<AdGroup> GetAllGroups()
        {
            return _adminRepository.GetAll();
        }

        public AdGroup GetGroupById(int id)
        {
            return _adminRepository.GetById(id);
        }

        public AdGroup CreateGroup(AdGroup group, string user)
        {
            if (_adminRepository.GetByName(group.GroupName) != null)
                throw new InvalidOperationException($"AD group '{group.GroupName}' is already configured.");

            group.IsActive = true;
            group.CreatedDate = DateTime.UtcNow;
            var created = _adminRepository.Add(group);
            _adminRepository.SaveChanges();

            _adminRepository.Log("Created", "ADGroup", created.GroupId, created.GroupName,
                $"AD group '{created.GroupName}' mapped to role '{created.Role}'", user);
            _adminRepository.SaveChanges();

            return created;
        }

        public AdGroup UpdateGroup(AdGroup group, string user)
        {
            var existing = _adminRepository.GetById(group.GroupId);
            if (existing == null)
                throw new InvalidOperationException($"AD group {group.GroupId} not found.");

            existing.GroupName = group.GroupName;
            existing.Role = group.Role;
            existing.Description = group.Description;
            existing.IsActive = group.IsActive;

            _adminRepository.SaveChanges();

            _adminRepository.Log("Updated", "ADGroup", existing.GroupId, existing.GroupName,
                $"AD group '{existing.GroupName}' updated", user);
            _adminRepository.SaveChanges();

            return existing;
        }

        public void DeleteGroup(int groupId, string user)
        {
            var group = _adminRepository.GetById(groupId);
            if (group == null)
                return;

            _adminRepository.Log("Deleted", "ADGroup", groupId, group.GroupName,
                $"AD group '{group.GroupName}' removed", user);
            _adminRepository.SaveChanges();

            _adminRepository.Remove(group);
            _adminRepository.SaveChanges();
        }

        public void ToggleGroupActive(int groupId, string user)
        {
            var group = _adminRepository.GetById(groupId);
            if (group == null)
                return;

            group.IsActive = !group.IsActive;
            _adminRepository.SaveChanges();

            _adminRepository.Log("Updated", "ADGroup", groupId, group.GroupName,
                $"AD group '{group.GroupName}' {(group.IsActive ? "activated" : "deactivated")}", user);
            _adminRepository.SaveChanges();
        }

        public IEnumerable<ActivityLogEntry> GetActivityLog(int take = 200)
        {
            return _adminRepository.GetActivityLog(take);
        }

        public IEnumerable<ActivityLogEntry> SearchActivityLog(string entityType, string action, string user)
        {
            return _adminRepository.SearchActivityLog(entityType, action, user);
        }

        public IDictionary<string, int> GetRoleAssignments()
        {
            return _adminRepository.GetRoleAssignments();
        }
    }
}
