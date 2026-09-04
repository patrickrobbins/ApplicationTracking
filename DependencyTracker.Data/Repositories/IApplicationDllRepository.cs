using System.Collections.Generic;
using DependencyTracker.Data.Models;

namespace DependencyTracker.Data.Repositories
{
    public interface IApplicationDllRepository : IRepository<ApplicationDll>
    {
        IEnumerable<ApplicationDll> GetForApplication(int applicationId);

        int CountForApplication(int applicationId);

        /// <summary>
        /// Finds DLL rows whose file name contains <paramref name="dllName"/> (case-insensitive)
        /// and, when a version target is supplied, whose version satisfies the comparison operator
        /// (=, &gt;, &gt;=, &lt;, &lt;=) against it. Returns an empty list when no name is given.
        /// </summary>
        IEnumerable<ApplicationDll> FindByDll(string dllName, string dllOperator, string version);
    }
}
