using System;
using System.Collections.Generic;
using System.Linq;
using DependencyTracker.Data.Models;
using DependencyTracker.Data.Repositories;

namespace DependencyTracker.Data.Services
{
    public class ApplicationPropertyService : IApplicationPropertyService
    {
        private readonly IApplicationPropertyRepository _propertyRepository;
        private readonly IAdminRepository _adminRepository;
        private readonly IApplicationRepository _applicationRepository;

        public ApplicationPropertyService()
            : this(new ApplicationPropertyRepository(), new AdminRepository(), new ApplicationRepository())
        {
        }

        public ApplicationPropertyService(
            IApplicationPropertyRepository propertyRepository,
            IAdminRepository adminRepository,
            IApplicationRepository applicationRepository)
        {
            _propertyRepository = propertyRepository;
            _adminRepository = adminRepository;
            _applicationRepository = applicationRepository;
        }

        public IEnumerable<ApplicationPropertyDefinition> GetDefinitions()
        {
            return _propertyRepository.GetDefinitions();
        }

        public IEnumerable<ApplicationPropertyDefinition> GetActiveDefinitions()
        {
            return _propertyRepository.GetActiveDefinitions();
        }

        public ApplicationPropertyDefinition GetDefinitionById(int id)
        {
            return _propertyRepository.GetById(id);
        }

        public ApplicationPropertyDefinition CreateDefinition(ApplicationPropertyDefinition definition, string user)
        {
            if (_propertyRepository.GetDefinitionByKey(definition.Key) != null)
                throw new InvalidOperationException($"A property definition with key '{definition.Key}' already exists.");

            definition.DataType = string.IsNullOrWhiteSpace(definition.DataType) ? "Text" : definition.DataType;
            definition.IsActive = true;
            definition.CreatedDate = DateTime.UtcNow;
            definition.ModifiedDate = DateTime.UtcNow;
            definition.CreatedBy = user;
            definition.ModifiedBy = user;

            var created = _propertyRepository.Add(definition);
            _propertyRepository.SaveChanges();

            _adminRepository.Log("Created", "ApplicationProperty", created.PropertyDefinitionId, created.Key,
                $"Property definition '{created.Key}' ({created.Label}) created", user);
            _adminRepository.SaveChanges();

            return created;
        }

        public ApplicationPropertyDefinition UpdateDefinition(ApplicationPropertyDefinition definition, string user)
        {
            var existing = _propertyRepository.GetById(definition.PropertyDefinitionId);
            if (existing == null)
                throw new InvalidOperationException($"Property definition {definition.PropertyDefinitionId} not found.");

            var duplicate = _propertyRepository.GetDefinitionByKey(definition.Key);
            if (duplicate != null && duplicate.PropertyDefinitionId != definition.PropertyDefinitionId)
                throw new InvalidOperationException($"A property definition with key '{definition.Key}' already exists.");

            existing.Key = definition.Key;
            existing.Label = definition.Label;
            existing.DataType = string.IsNullOrWhiteSpace(definition.DataType) ? "Text" : definition.DataType;
            existing.ScanPattern = definition.ScanPattern;
            existing.Description = definition.Description;
            existing.IsActive = definition.IsActive;
            existing.SortOrder = definition.SortOrder;
            existing.ModifiedDate = DateTime.UtcNow;
            existing.ModifiedBy = user;

            _propertyRepository.SaveChanges();

            _adminRepository.Log("Updated", "ApplicationProperty", existing.PropertyDefinitionId, existing.Key,
                $"Property definition '{existing.Key}' updated", user);
            _adminRepository.SaveChanges();

            return existing;
        }

        public void DeleteDefinition(int definitionId, string user)
        {
            var definition = _propertyRepository.GetById(definitionId);
            if (definition == null)
                return;

            _adminRepository.Log("Deleted", "ApplicationProperty", definitionId, definition.Key,
                $"Property definition '{definition.Key}' removed", user);
            _adminRepository.SaveChanges();

            _propertyRepository.Remove(definition);
            _propertyRepository.SaveChanges();
        }

        public void ToggleDefinitionActive(int definitionId, string user)
        {
            var definition = _propertyRepository.GetById(definitionId);
            if (definition == null)
                return;

            definition.IsActive = !definition.IsActive;
            definition.ModifiedDate = DateTime.UtcNow;
            definition.ModifiedBy = user;
            _propertyRepository.SaveChanges();

            _adminRepository.Log("Updated", "ApplicationProperty", definitionId, definition.Key,
                $"Property definition '{definition.Key}' {(definition.IsActive ? "activated" : "deactivated")}", user);
            _adminRepository.SaveChanges();
        }

        public IEnumerable<ApplicationPropertyValue> GetValuesForApplication(int applicationId)
        {
            return _propertyRepository.GetValuesForApplication(applicationId);
        }

        public IEnumerable<ApplicationPropertyValue> GetAllValues()
        {
            return _propertyRepository.GetAllValues();
        }

        public void SaveValuesForApplication(int applicationId, IDictionary<int, string> valuesByDefinition, string user)
        {
            if (valuesByDefinition == null)
                return;

            var application = _applicationRepository.GetById(applicationId);
            var existingValues = _propertyRepository.GetValuesForApplication(applicationId).ToList();

            foreach (var kvp in valuesByDefinition)
            {
                var existing = existingValues.FirstOrDefault(v => v.PropertyDefinitionId == kvp.Key);
                var value = kvp.Value;

                if (existing != null)
                {
                    if (existing.Value != value)
                    {
                        existing.Value = value;
                        existing.ModifiedDate = DateTime.UtcNow;
                        existing.ModifiedBy = user;
                        _propertyRepository.SaveChanges();

                        LogValueChange(application, existing.Definition, value, user);
                    }
                }
                else if (!string.IsNullOrWhiteSpace(value))
                {
                    var created = _propertyRepository.AddValue(new ApplicationPropertyValue
                    {
                        ApplicationId = applicationId,
                        PropertyDefinitionId = kvp.Key,
                        Value = value,
                        ModifiedDate = DateTime.UtcNow,
                        ModifiedBy = user
                    });
                    _propertyRepository.SaveChanges();

                    LogValueChange(application, created.Definition, value, user);
                }
            }
        }

        private void LogValueChange(Application application, ApplicationPropertyDefinition definition, string value, string user)
        {
            if (application == null || definition == null)
                return;

            var label = definition.Label ?? definition.Key;
            _adminRepository.Log("Updated", "ApplicationProperty", application.ApplicationId,
                $"{application.Name} / {label}",
                $"Property '{label}' of application '{application.Name}' set to '{(string.IsNullOrWhiteSpace(value) ? "-" : value)}'",
                user);
            _adminRepository.SaveChanges();
        }
    }
}
