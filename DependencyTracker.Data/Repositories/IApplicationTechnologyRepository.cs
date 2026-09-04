using System.Collections.Generic;
using DependencyTracker.Data.Models;

namespace DependencyTracker.Data.Repositories
{
    public interface IApplicationTechnologyRepository : IRepository<ApplicationTechnology>
    {
        IEnumerable<ApplicationTechnology> GetTechnologies();
        IEnumerable<ApplicationTechnology> GetActiveTechnologies();
        ApplicationTechnology GetByName(string name);
        IEnumerable<ApplicationTechnology> GetForApplication(int applicationId);
        ApplicationTechnologyMapping GetMapping(int applicationId, int technologyId);
        ApplicationTechnologyMapping AddMapping(int applicationId, int technologyId);
        void RemoveMapping(ApplicationTechnologyMapping mapping);
        IEnumerable<ApplicationTechnologyMapping> GetMappingsForApplication(int applicationId);
        IEnumerable<ApplicationTechnologyMapping> GetMappingsForTechnology(int technologyId);
    }
}
