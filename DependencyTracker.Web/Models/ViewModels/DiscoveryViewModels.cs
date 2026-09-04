using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace DependencyTracker.Web.Models.ViewModels
{
    /// <summary>
    /// Step 1: scan parameters plus the review table of discovered applications.
    /// </summary>
    public class DiscoveryIndexViewModel
    {
        [Required(ErrorMessage = "Enter the base directory or UNC path to scan.")]
        public string BasePath { get; set; }

        /// <summary>
        /// Optional path to applicationHost.config (default IIS:
        /// C:\Windows\System32\inetsrv\config\applicationHost.config). Used only to
        /// resolve IIS site names; the folder scan works without it.
        /// </summary>
        public string AppHostConfigPath { get; set; }

        /// <summary>
        /// Optional credentials used to reach network shares / remote physical paths that
        /// the application's own identity cannot access. When blank, the scan runs as the
        /// application's Windows identity.
        /// </summary>
        public string Username { get; set; }

        public string Password { get; set; }

        public bool Scanned { get; set; }
        public string Warning { get; set; }
        public List<DiscoveredAppViewModel> Apps { get; set; }
    }

    /// <summary>One discovered application row in the review table.</summary>
    public class DiscoveredAppViewModel
    {
        public bool Include { get; set; }
        public string Name { get; set; }
        public string SourcePath { get; set; }
        public string SiteName { get; set; }
        public bool HasConfig { get; set; }

        /// <summary>Number of DLLs found under the source path (bin and subdirectories).</summary>
        public int DllCount { get; set; }

        /// <summary>
        /// Existing application to UPDATE (value &gt; 0) or 0 to CREATE a new one.
        /// </summary>
        public int MatchedApplicationId { get; set; }

        /// <summary>"Create new application" option followed by suggested matches.</summary>
        public IEnumerable<SelectListItem> MatchOptions { get; set; }
    }

    /// <summary>
    /// Step 2: dependency candidates for each confirmed application, before Apply.
    /// </summary>
    public class DiscoveryReviewViewModel
    {
        public string BasePath { get; set; }
        public string AppHostConfigPath { get; set; }
        public List<DiscoveryAppReviewViewModel> Apps { get; set; }
    }

    public class DiscoveryAppReviewViewModel
    {
        public string Name { get; set; }
        public string SourcePath { get; set; }
        public string SiteName { get; set; }
        public int MatchedApplicationId { get; set; }

        /// <summary>Number of DLLs that will be recorded for this application on Apply.</summary>
        public int DllCount { get; set; }

        public List<ConfigScanCandidateViewModel> Candidates { get; set; }
    }
}
