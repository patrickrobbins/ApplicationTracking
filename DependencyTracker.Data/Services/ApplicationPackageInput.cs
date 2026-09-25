using System;

namespace DependencyTracker.Data.Services
{
    /// <summary>
    /// One package row captured from the application form's inline editor.
    /// The package itself is resolved (or created) in the shared pool keyed by
    /// (PackageTypeId, Name); the Version is stored on the per-application
    /// mapping.
    /// </summary>
    public class ApplicationPackageInput
    {
        public int PackageTypeId { get; set; }
        public string Name { get; set; }
        public string Version { get; set; }

        /// <summary>
        /// Stable key used to de-duplicate form rows case-insensitively.
        /// </summary>
        public string DedupeKey
        {
            get { return PackageTypeId + "|" + (Name ?? string.Empty).Trim().ToLowerInvariant(); }
        }
    }
}