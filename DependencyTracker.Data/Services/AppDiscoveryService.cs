using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace DependencyTracker.Data.Services
{
    /// <summary>
    /// An application discovered by scanning a base directory (local path or UNC
    /// share) for configuration files. Optionally enriched with the IIS site name
    /// when applicationHost.config is readable.
    /// </summary>
    public class DiscoveredApplication
    {
        /// <summary>Best-effort application name (project file, site name, or folder name).</summary>
        public string Name { get; set; }

        /// <summary>IIS site name when applicationHost.config is available and matched, otherwise null.</summary>
        public string SiteName { get; set; }

        /// <summary>Full physical path (directory) of the discovered application.</summary>
        public string SourcePath { get; set; }

        /// <summary>True when at least one web.config/app.config was found in the folder.</summary>
        public bool HasConfig { get; set; }

        /// <summary>Full paths of the config files found in the folder (web.config preferred, then app.config).</summary>
        public List<string> ConfigFilePaths { get; set; }

        /// <summary>DLLs found in the folder and its subdirectories (e.g. bin), with deployed versions.</summary>
        public List<DiscoveredDll> DllFiles { get; set; }
    }

    /// <summary>
    /// A library (DLL) found under an application's source path, with the deployed file version.
    /// </summary>
    public class DiscoveredDll
    {
        /// <summary>File name, e.g. Newtonsoft.Json.dll.</summary>
        public string FileName { get; set; }

        /// <summary>File version, e.g. 13.0.3.27908. Null when none was readable.</summary>
        public string Version { get; set; }

        /// <summary>Path relative to the application source path, e.g. bin\Newtonsoft.Json.dll.</summary>
        public string RelativePath { get; set; }
    }

    /// <summary>
    /// Discovers applications on an IIS host / network share by recursively finding
    /// web.config / app.config files (each containing folder is a candidate app) and,
    /// when available, parsing applicationHost.config to resolve IIS site names.
    /// </summary>
    public class AppDiscoveryService : IAppDiscoveryService
    {
        private static readonly HashSet<string> ExcludedDirectories =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "bin", "obj", "packages", "node_modules", ".git", ".svn", ".hg",
                ".vs", "vs", "Properties", "App_Data", "TestResults", "Test", "Tests",
                "bower_components", "wwwroot", "dist", "build"
            };

        private static readonly string[] ConfigFileNames = { "web.config", "app.config" };

        /// <summary>
        /// Directories skipped while walking for DLLs. Note: unlike the config-file walk,
        /// "bin" is intentionally NOT excluded - deployed libraries live in bin and must be
        /// collected. "obj" (build intermediates) stays excluded.
        /// </summary>
        private static readonly HashSet<string> DllExcludedDirectories =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "obj", "packages", "node_modules", ".git", ".svn", ".hg",
                ".vs", "vs", "Properties", "App_Data", "TestResults", "Test", "Tests",
                "bower_components", "wwwroot", "dist", "build"
            };

        /// <summary>
        /// Discovers candidate applications under <paramref name="basePath"/> (recursively).
        /// When <paramref name="appHostConfigPath"/> is provided and readable, site names are
        /// resolved from applicationHost.config and any site whose physical path has no
        /// config file is also included.
        /// </summary>
        public List<DiscoveredApplication> Discover(string basePath, string appHostConfigPath, DiscoveryCredentials credentials = null)
        {
            List<DiscoveredApplication> result = null;
            string error;
            if (!DiscoveryImpersonation.TryRun(credentials, () =>
            {
                result = DiscoverCore(basePath, appHostConfigPath);
            }, out error))
            {
                throw new UnauthorizedAccessException(
                    "Could not log on with the supplied discovery credentials to run the scan. " + error);
            }
            return result;
        }

        private List<DiscoveredApplication> DiscoverCore(string basePath, string appHostConfigPath)
        {
            var result = new Dictionary<string, DiscoveredApplication>(StringComparer.OrdinalIgnoreCase);
            var siteNames = ReadSiteNames(appHostConfigPath);

            if (!string.IsNullOrWhiteSpace(basePath) && Directory.Exists(basePath))
            {
                foreach (var configFile in EnumerateConfigFiles(basePath))
                {
                    var folder = Path.GetDirectoryName(configFile);
                    if (string.IsNullOrEmpty(folder))
                        continue;

                    var key = NormalizePath(folder);
                    if (result.ContainsKey(key))
                        continue;

                    string siteName = null;
                    siteNames.TryGetValue(key, out siteName);
                    result[key] = BuildFromFolder(folder, siteName);
                }
            }

            // Sites whose physical path exists but contained no config file: still surface
            // them so an app can be registered without dependency scanning.
            foreach (var pair in siteNames)
            {
                if (Directory.Exists(pair.Key) && !result.ContainsKey(pair.Key))
                    result[pair.Key] = BuildFromFolder(pair.Key, pair.Value);
            }

            return result.Values
                .OrderBy(a => a.Name, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        /// <summary>
        /// Reads the config files for a single application folder (used on the
        /// dependency-review step, which must not rely on previously posted XML).
        /// </summary>
        public List<string> GetConfigFilePaths(string appPath, DiscoveryCredentials credentials = null)
        {
            List<string> result = null;
            string error;
            if (!DiscoveryImpersonation.TryRun(credentials, () =>
            {
                result = GetConfigFilePathsCore(appPath);
            }, out error))
            {
                throw new UnauthorizedAccessException(
                    "Could not log on with the supplied discovery credentials. " + error);
            }
            return result;
        }

        private List<string> GetConfigFilePathsCore(string appPath)
        {
            var folder = appPath;
            if (!string.IsNullOrWhiteSpace(folder) && Directory.Exists(folder))
            {
                try
                {
                    return Directory.GetFiles(folder)
                        .Where(f => IsConfigFile(Path.GetFileName(f)))
                        .OrderBy(Path.GetFileName, StringComparer.OrdinalIgnoreCase)
                        .ToList();
                }
                catch (Exception)
                {
                    // fall through to empty
                }
            }
            return new List<string>();
        }

        /// <summary>
        /// Recursively collects all *.dll files under <paramref name="appPath"/> (including
        /// subdirectories such as bin) with their deployed file versions. Directories that are
        /// build/intermediate artifacts are skipped. Returns an empty list when the path is
        /// missing or unreadable.
        /// </summary>
        public List<DiscoveredDll> GetDllFiles(string appPath, DiscoveryCredentials credentials = null)
        {
            List<DiscoveredDll> result = null;
            string error;
            if (!DiscoveryImpersonation.TryRun(credentials, () =>
            {
                result = GetDllFilesCore(appPath);
            }, out error))
            {
                throw new UnauthorizedAccessException(
                    "Could not log on with the supplied discovery credentials. " + error);
            }
            return result;
        }

        private List<DiscoveredDll> GetDllFilesCore(string appPath)
        {
            var result = new List<DiscoveredDll>();
            if (string.IsNullOrWhiteSpace(appPath) || !Directory.Exists(appPath))
                return result;

            var stack = new Stack<string>();
            stack.Push(appPath);

            while (stack.Count > 0)
            {
                var directory = stack.Pop();
                var directoryName = Path.GetFileName(directory.TrimEnd('\\', '/'));
                if (!string.IsNullOrEmpty(directoryName) && DllExcludedDirectories.Contains(directoryName))
                    continue;

                foreach (var dllFile in SafeGetFiles(directory, "*.dll"))
                {
                    var fileName = Path.GetFileName(dllFile);
                    if (string.IsNullOrEmpty(fileName))
                        continue;

                    string version = null;
                    try
                    {
                        version = System.Diagnostics.FileVersionInfo.GetVersionInfo(dllFile).FileVersion;
                    }
                    catch (Exception)
                    {
                        // not a PE file or unreadable - keep the row without a version
                    }

                    result.Add(new DiscoveredDll
                    {
                        FileName = fileName,
                        Version = version,
                        RelativePath = GetRelativePath(appPath, dllFile)
                    });
                }

                foreach (var subDirectory in SafeGetDirectories(directory))
                    stack.Push(subDirectory);
            }

            return result
                .OrderBy(d => d.FileName, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        /// <summary>
        /// Case-insensitive full path normalized for comparison (trailing slashes removed,
        /// environment variables expanded). Returns null for blank input.
        /// </summary>
        public string NormalizePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return null;

            try
            {
                path = Environment.ExpandEnvironmentVariables(path);
            }
            catch (Exception)
            {
                // ignore
            }

            try
            {
                path = Path.GetFullPath(path);
            }
            catch (Exception)
            {
                // ignore; use as-is
            }

            return path.TrimEnd('\\', '/');
        }

        private Dictionary<string, string> ReadSiteNames(string appHostConfigPath)
        {
            var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (string.IsNullOrWhiteSpace(appHostConfigPath) || !File.Exists(appHostConfigPath))
                return map;

            try
            {
                var document = XDocument.Load(appHostConfigPath);
                foreach (var site in document.Descendants("site"))
                {
                    var name = (string)site.Attribute("name");
                    var application = site.Elements("application").FirstOrDefault();
                    var virtualDirectory = application == null
                        ? null
                        : application.Elements("virtualDirectory").FirstOrDefault();
                    var physicalPath = virtualDirectory == null
                        ? null
                        : (string)virtualDirectory.Attribute("physicalPath");
                    if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(physicalPath))
                        continue;

                    var key = NormalizePath(physicalPath);
                    if (key != null && !map.ContainsKey(key))
                        map[key] = name;
                }
            }
            catch (Exception)
            {
                // applicationHost.config is unreadable or malformed - folder scan still works.
            }

            return map;
        }

        private DiscoveredApplication BuildFromFolder(string folder, string siteName)
        {
            var configFiles = new List<string>();
            try
            {
                configFiles = Directory.GetFiles(folder)
                    .Where(f => IsConfigFile(Path.GetFileName(f)))
                    .OrderBy(Path.GetFileName, StringComparer.OrdinalIgnoreCase)
                    .ToList();
            }
            catch (Exception)
            {
                // ignore
            }

            return new DiscoveredApplication
            {
                Name = DeriveName(folder, siteName),
                SiteName = siteName,
                SourcePath = folder,
                HasConfig = configFiles.Count > 0,
                ConfigFilePaths = configFiles,
                DllFiles = GetDllFiles(folder)
            };
        }

        private string DeriveName(string folder, string siteName)
        {
            try
            {
                var project = Directory.GetFiles(folder)
                    .FirstOrDefault(f => f.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase)
                                         || f.EndsWith(".vbproj", StringComparison.OrdinalIgnoreCase));
                if (project != null)
                    return Path.GetFileNameWithoutExtension(project);
            }
            catch (Exception)
            {
                // ignore
            }

            if (!string.IsNullOrWhiteSpace(siteName))
                return siteName;

            var leaf = Path.GetFileName(folder.TrimEnd('\\', '/'));
            if (!string.IsNullOrWhiteSpace(leaf))
                return leaf;

            return folder;
        }

        private static bool IsConfigFile(string fileName)
        {
            return ConfigFileNames.Any(n => string.Equals(n, fileName, StringComparison.OrdinalIgnoreCase));
        }

        private IEnumerable<string> EnumerateConfigFiles(string root)
        {
            var stack = new Stack<string>();
            stack.Push(root);

            while (stack.Count > 0)
            {
                var directory = stack.Pop();
                var directoryName = Path.GetFileName(directory.TrimEnd('\\', '/'));
                if (!string.IsNullOrEmpty(directoryName) && ExcludedDirectories.Contains(directoryName))
                    continue;

                foreach (var configFile in SafeGetFiles(directory))
                {
                    if (IsConfigFile(Path.GetFileName(configFile)))
                        yield return configFile;
                }

                foreach (var subDirectory in SafeGetDirectories(directory))
                    stack.Push(subDirectory);
            }
        }

        private static IEnumerable<string> SafeGetFiles(string directory)
        {
            try
            {
                return Directory.GetFiles(directory);
            }
            catch (Exception)
            {
                return Enumerable.Empty<string>();
            }
        }

        private static IEnumerable<string> SafeGetFiles(string directory, string searchPattern)
        {
            try
            {
                return Directory.GetFiles(directory, searchPattern);
            }
            catch (Exception)
            {
                return Enumerable.Empty<string>();
            }
        }

        /// <summary>
        /// Returns <paramref name="fullPath"/> relative to <paramref name="rootPath"/>
        /// (case-insensitive), or the file name when the path is not under the root.
        /// </summary>
        private static string GetRelativePath(string rootPath, string fullPath)
        {
            try
            {
                var root = NormalizePathCore(rootPath);
                var full = NormalizePathCore(fullPath);
                if (root != null && full != null && full.StartsWith(root, StringComparison.OrdinalIgnoreCase))
                {
                    var relative = full.Substring(root.Length).TrimStart('\\', '/');
                    if (relative.Length > 0)
                        return relative;
                }
            }
            catch (Exception)
            {
                // fall through
            }
            return Path.GetFileName(fullPath);
        }

        private static string NormalizePathCore(string path)
        {
            try
            {
                path = Environment.ExpandEnvironmentVariables(path);
                return Path.GetFullPath(path);
            }
            catch (Exception)
            {
                return path;
            }
        }

        private static IEnumerable<string> SafeGetDirectories(string directory)
        {
            try
            {
                return Directory.GetDirectories(directory);
            }
            catch (Exception)
            {
                return Enumerable.Empty<string>();
            }
        }
    }
}
