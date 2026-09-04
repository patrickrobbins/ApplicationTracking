using System.Collections.Generic;

namespace DependencyTracker.Data.Services
{
    public interface IAppDiscoveryService
    {
        /// <summary>
        /// Recursively discovers candidate applications under <paramref name="basePath"/>
        /// (a local directory or UNC share). When <paramref name="appHostConfigPath"/> is
        /// provided and readable, IIS site names are resolved and sites without a config
        /// file are included as well.
        /// </summary>
        List<DiscoveredApplication> Discover(string basePath, string appHostConfigPath, DiscoveryCredentials credentials = null);

        /// <summary>Reads the config file paths for a single application folder.</summary>
        List<string> GetConfigFilePaths(string appPath, DiscoveryCredentials credentials = null);

        /// <summary>
        /// Recursively collects the *.dll files (with versions) for a single application folder.
        /// </summary>
        List<DiscoveredDll> GetDllFiles(string appPath, DiscoveryCredentials credentials = null);

        /// <summary>Normalizes a path for comparison (case-insensitive, trailing slashes removed).</summary>
        string NormalizePath(string path);
    }
}
