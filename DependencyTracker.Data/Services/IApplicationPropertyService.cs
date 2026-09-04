using System.Collections.Generic;
using DependencyTracker.Data.Models;

namespace DependencyTracker.Data.Services
{
    public interface IApplicationPropertyService
    {
        IEnumerable<ApplicationPropertyDefinition> GetDefinitions();
        IEnumerable<ApplicationPropertyDefinition> GetActiveDefinitions();
        ApplicationPropertyDefinition GetDefinitionById(int id);
        ApplicationPropertyDefinition CreateDefinition(ApplicationPropertyDefinition definition, string user);
        ApplicationPropertyDefinition UpdateDefinition(ApplicationPropertyDefinition definition, string user);
        void DeleteDefinition(int definitionId, string user);
        void ToggleDefinitionActive(int definitionId, string user);
        IEnumerable<ApplicationPropertyValue> GetValuesForApplication(int applicationId);
        IEnumerable<ApplicationPropertyValue> GetAllValues();
        void SaveValuesForApplication(int applicationId, IDictionary<int, string> valuesByDefinition, string user);
    }
}
