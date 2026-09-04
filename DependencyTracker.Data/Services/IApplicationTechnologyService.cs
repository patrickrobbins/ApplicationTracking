using System.Collections.Generic;
using DependencyTracker.Data.Models;

namespace DependencyTracker.Data.Services
{
    public interface IApplicationTechnologyService
    {
        IEnumerable<ApplicationTechnology> GetTechnologies();
        IEnumerable<ApplicationTechnology> GetActiveTechnologies();
        ApplicationTechnology GetById(int id);
        ApplicationTechnology GetByName(string name);
        ApplicationTechnology Create(ApplicationTechnology technology, string user);
        ApplicationTechnology Update(ApplicationTechnology technology, string user);
        void ToggleActive(int technologyId, string user);
        void Delete(int technologyId, string user);
        IEnumerable<ApplicationTechnology> GetForApplication(int applicationId);
        void SetForApplication(int applicationId, IEnumerable<int> technologyIds, string user);
    }
}
