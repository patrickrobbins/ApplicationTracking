using System;
using System.Collections.Generic;
using System.Linq;
using DependencyTracker.Data.Models;
using DependencyTracker.Data.Repositories;

namespace DependencyTracker.Data.Services
{
    public class DependencyService : IDependencyService
    {
        private static readonly string[] DependencyTypes =
            { "API", "Database", "File", "Message", "UI", "Infrastructure" };
        private static readonly string[] Frequencies =
            { "Real-time", "Hourly", "Daily", "Weekly", "Monthly" };
        private static readonly string[] Directions =
            { "Upstream", "Downstream", "Bidirectional" };
        private static readonly string[] CriticalityLevels =
            { "Critical", "High", "Medium", "Low" };

        private readonly IDependencyRepository _dependencyRepository;
        private readonly IAdminRepository _adminRepository;
        private readonly IApplicationRepository _applicationRepository;

        public DependencyService()
            : this(new DependencyRepository(), new AdminRepository(), new ApplicationRepository())
        {
        }

        public DependencyService(
            IDependencyRepository dependencyRepository,
            IAdminRepository adminRepository,
            IApplicationRepository applicationRepository)
        {
            _dependencyRepository = dependencyRepository;
            _adminRepository = adminRepository;
            _applicationRepository = applicationRepository;
        }

        public Dependency GetById(int id)
        {
            return _dependencyRepository.GetById(id);
        }

        public IEnumerable<Dependency> GetAll()
        {
            return _dependencyRepository.GetAll();
        }

        public IEnumerable<Dependency> GetForApplication(int applicationId)
        {
            return _dependencyRepository.GetForApplication(applicationId);
        }

        public IEnumerable<Dependency> GetUpstream(int applicationId)
        {
            return _dependencyRepository.GetUpstream(applicationId);
        }

        public IEnumerable<Dependency> GetDownstream(int applicationId)
        {
            return _dependencyRepository.GetDownstream(applicationId);
        }

        public IEnumerable<Dependency> Search(string dependencyType, string criticality, int? sourceId, int? targetId)
        {
            return _dependencyRepository.Search(dependencyType, criticality, sourceId, targetId);
        }

        public Dependency Create(Dependency dependency, string user)
        {
            if (dependency.SourceApplicationId == dependency.TargetApplicationId)
                throw new InvalidOperationException("An application cannot depend on itself.");

            if (RelationshipExists(dependency.SourceApplicationId, dependency.TargetApplicationId, dependency.DependencyType))
                throw new InvalidOperationException("This dependency relationship already exists.");

            dependency.CreatedDate = DateTime.UtcNow;
            dependency.ModifiedDate = DateTime.UtcNow;
            dependency.CreatedBy = user;
            dependency.ModifiedBy = user;

            var created = _dependencyRepository.Add(dependency);
            _dependencyRepository.SaveChanges();

            var source = _applicationRepository.GetById(dependency.SourceApplicationId);
            var target = _applicationRepository.GetById(dependency.TargetApplicationId);

            _adminRepository.Log("Created", "Dependency", created.DependencyId,
                $"{source?.Name} -> {target?.Name}",
                $"Dependency of type '{dependency.DependencyType}' created between '{source?.Name}' and '{target?.Name}'", user);
            _adminRepository.SaveChanges();

            return created;
        }

        public Dependency Update(Dependency dependency, string user)
        {
            var existing = _dependencyRepository.GetById(dependency.DependencyId);
            if (existing == null)
                throw new InvalidOperationException($"Dependency {dependency.DependencyId} not found.");

            existing.SourceApplicationId = dependency.SourceApplicationId;
            existing.TargetApplicationId = dependency.TargetApplicationId;
            existing.DependencyType = dependency.DependencyType;
            existing.Direction = dependency.Direction;
            existing.Description = dependency.Description;
            existing.CriticalityLevel = dependency.CriticalityLevel;
            existing.Impact = dependency.Impact;
            existing.Frequency = dependency.Frequency;
            existing.ModifiedDate = DateTime.UtcNow;
            existing.ModifiedBy = user;

            _dependencyRepository.SaveChanges();

            _adminRepository.Log("Updated", "Dependency", existing.DependencyId,
                $"{existing.SourceApplicationId} -> {existing.TargetApplicationId}",
                $"Dependency {existing.DependencyId} updated", user);
            _adminRepository.SaveChanges();

            return existing;
        }

        public void Delete(int dependencyId, string user)
        {
            var dependency = _dependencyRepository.GetById(dependencyId);
            if (dependency == null)
                return;

            var source = _applicationRepository.GetById(dependency.SourceApplicationId);
            var target = _applicationRepository.GetById(dependency.TargetApplicationId);

            _adminRepository.Log("Deleted", "Dependency", dependencyId,
                $"{source?.Name} -> {target?.Name}",
                $"Dependency of type '{dependency.DependencyType}' deleted between '{source?.Name}' and '{target?.Name}'", user);
            _adminRepository.SaveChanges();

            _dependencyRepository.Remove(dependency);
            _dependencyRepository.SaveChanges();
        }

        public bool RelationshipExists(int sourceId, int targetId, string dependencyType)
        {
            return _dependencyRepository.RelationshipExists(sourceId, targetId, dependencyType);
        }

        public IEnumerable<DependencyChainResult> GetChain(int applicationId, int maxDepth, string direction)
        {
            return _dependencyRepository.GetChain(applicationId, maxDepth, direction);
        }

        public IEnumerable<string> GetAllTypes()
        {
            return DependencyTypes;
        }

        public IEnumerable<string> GetAllFrequencies()
        {
            return Frequencies;
        }

        public IEnumerable<string> GetAllDirections()
        {
            return Directions;
        }

        public IEnumerable<string> GetAllCriticalityLevels()
        {
            return CriticalityLevels;
        }
    }
}
