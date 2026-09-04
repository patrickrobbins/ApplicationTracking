using System.Collections.Generic;
using DependencyTracker.Data.Models;

namespace DependencyTracker.Data.Services
{
    public interface IApplicationDllService
    {
        IEnumerable<ApplicationDll> GetForApplication(int applicationId);

        int CountForApplication(int applicationId);

        /// <summary>
        /// Replaces the application's DLL inventory with the discovered set.
        /// Returns the number of unique DLLs stored.
        /// </summary>
        int SyncFromDiscovery(int applicationId, string applicationName, IEnumerable<DiscoveredDll> dlls, string user);

        /// <summary>
        /// Finds DLL rows by file name (case-insensitive contains) with an optional
        /// version comparison (=, &gt;, &gt;=, &lt;, &lt;=).
        /// </summary>
        IEnumerable<ApplicationDll> FindByDll(string dllName, string dllOperator, string version);
    }
}
