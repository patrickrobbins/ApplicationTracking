using System.Collections.Generic;
using DependencyTracker.Data.Models;

namespace DependencyTracker.Data.Services
{
    public interface IConfigScannerService
    {
        IEnumerable<ConfigScanCandidate> Scan(
            string configXml,
            IEnumerable<ApplicationPropertyDefinition> activeDefinitions,
            IEnumerable<ApplicationPropertyValue> allPropertyValues,
            IEnumerable<Application> applications);
    }
}
