using System.Collections.Generic;
using DependencyTracker.Data.Models;

namespace DependencyTracker.Data.Repositories
{
    public interface IApplicationPropertyRepository : IRepository<ApplicationPropertyDefinition>
    {
        IEnumerable<ApplicationPropertyDefinition> GetDefinitions();
        IEnumerable<ApplicationPropertyDefinition> GetActiveDefinitions();
        ApplicationPropertyDefinition GetDefinitionByKey(string key);
        IEnumerable<ApplicationPropertyValue> GetValuesForApplication(int applicationId);
        IEnumerable<ApplicationPropertyValue> GetAllValues();
        ApplicationPropertyValue AddValue(ApplicationPropertyValue value);
    }
}
