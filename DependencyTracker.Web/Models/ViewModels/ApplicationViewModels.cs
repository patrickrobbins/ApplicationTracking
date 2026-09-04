using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using DependencyTracker.Data.Models;

namespace DependencyTracker.Web.Models.ViewModels
{
    public class ApplicationListItemViewModel
    {
        public int ApplicationId { get; set; }
        public string Name { get; set; }
        public string Version { get; set; }
        public string Description { get; set; }
        public string Environment { get; set; }
        public string CriticalityLevel { get; set; }
        public string Status { get; set; }
        public string Category { get; set; }
        public string Family { get; set; }
        public string Team { get; set; }
        public string TagNames { get; set; }
        public int DependencyCount { get; set; }

        /// <summary>Matched DLLs (file name + version) when the search included a DLL filter.</summary>
        public IEnumerable<string> MatchedDlls { get; set; }

        /// <summary>True when this row represents a soft-deleted application.</summary>
        public bool IsDeleted { get; set; }

        /// <summary>
        /// True when this row is the highest-version row of a multi-version group.
        /// Used to render the expand/collapse toggle on the grouped index.
        /// </summary>
        public bool IsGroupHead { get; set; }

        /// <summary>Number of versions of this application name in the current result set.</summary>
        public int VersionCount { get; set; }

        /// <summary>
        /// Versions of the sibling rows in this group (excluding this row), for the
        /// "show all N versions" hint on the group head. Null for single-version rows.
        /// </summary>
        public string GroupVersions { get; set; }

        /// <summary>Whether the index is currently expanded to show every version.</summary>
        public bool ShowAllVersions { get; set; }
    }

    public class ApplicationSearchViewModel
    {
        public string Term { get; set; }
        public string Environment { get; set; }
        public string Status { get; set; }
        public string Criticality { get; set; }
        public int? CategoryId { get; set; }

        /// <summary>Application technology tag filter (exact tag id).</summary>
        public int? TechnologyId { get; set; }

        /// <summary>Application family filter (exact id).</summary>
        public int? FamilyId { get; set; }

        /// <summary>Shared user tag filter (exact id).</summary>
        public int? TagId { get; set; }

        /// <summary>Result ordering: "" (name) or "family".</summary>
        public string SortBy { get; set; }

        /// <summary>Application version filter (exact match), e.g. 1.2.0.</summary>
        public string Version { get; set; }

        /// <summary>DLL file name filter (case-insensitive contains), e.g. Newtonsoft.Json.dll.</summary>
        public string DllName { get; set; }

        /// <summary>Version comparison operator: "" (any), "=", "&gt;", "&gt;=", "&lt;", "&lt;=".</summary>
        public string DllOperator { get; set; }

        /// <summary>Version to compare DLLs against, e.g. 13.0.0.</summary>
        public string DllVersion { get; set; }

        public IEnumerable<SelectListItem> DllOperators { get; set; }

        public IEnumerable<string> Environments { get; set; }
        public IEnumerable<string> Statuses { get; set; }
        public IEnumerable<string> Criticalities { get; set; }
        public IEnumerable<string> Versions { get; set; }
        public IEnumerable<SelectListItem> Categories { get; set; }
        public IEnumerable<SelectListItem> Technologies { get; set; }
        public IEnumerable<SelectListItem> Families { get; set; }
        public IEnumerable<SelectListItem> Tags { get; set; }
        public IEnumerable<SelectListItem> SortOptions { get; set; }
        public IEnumerable<ApplicationListItemViewModel> Results { get; set; }
        public bool CanEdit { get; set; }

        /// <summary>Include soft-deleted applications in the results.</summary>
        public bool IncludeDeleted { get; set; }

        /// <summary>Show every version as its own row instead of only the highest per name.</summary>
        public bool ShowAllVersions { get; set; }
    }

    public class ApplicationFormViewModel
    {
        public int ApplicationId { get; set; }

        [Required]
        [StringLength(200)]
        [Remote("ValidateName", "Applications", HttpMethod = "POST", AdditionalFields = "ApplicationId,Version")]
        public string Name { get; set; }

        [StringLength(50)]
        [Display(Name = "Version")]
        public string Version { get; set; }

        [StringLength(2000)]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; }

        [StringLength(200)]
        public string BusinessGroup { get; set; }

        [StringLength(200)]
        public string BusinessOwner { get; set; }

        [StringLength(200)]
        [EmailAddress(ErrorMessage = "Business owner email is not a valid email address.")]
        public string BusinessOwnerEmail { get; set; }

        [StringLength(200)]
        public string BusinessBackup { get; set; }

        [StringLength(200)]
        [EmailAddress(ErrorMessage = "Business backup email is not a valid email address.")]
        public string BusinessBackupEmail { get; set; }

        [StringLength(200)]
        [EmailAddress(ErrorMessage = "Technical owner email is not a valid email address.")]
        public string TechnicalOwnerEmail { get; set; }

        public string Environment { get; set; }

        public string CriticalityLevel { get; set; }

        [Display(Name = "Default dependency criticality")]
        public string DefaultDependencyCriticality { get; set; }

        [StringLength(2000)]
        [DataType(DataType.MultilineText)]
        [Display(Name = "Default dependency impact")]
        public string DefaultDependencyImpact { get; set; }

        public string Status { get; set; }

        public int? CategoryId { get; set; }

        public int? FamilyId { get; set; }

        public int? TechnicalOwnershipTeamId { get; set; }

        [StringLength(500)]
        [DataType(DataType.Url)]
        public string ExternalUrl { get; set; }

        [StringLength(500)]
        [Display(Name = "Source path")]
        public string SourcePath { get; set; }

        [StringLength(200)]
        [Display(Name = "Application IDE")]
        public string ApplicationIDE { get; set; }

        [StringLength(100)]
        [Display(Name = "Framework version")]
        public string FrameworkVersion { get; set; }

        [StringLength(500)]
        [DataType(DataType.Url)]
        [Display(Name = "Documentation link")]
        public string DocumentationLink { get; set; }

        [StringLength(500)]
        [Display(Name = "Source control location")]
        public string SourceControlLocation { get; set; }

        [StringLength(200)]
        [Display(Name = "Hours of operation")]
        public string HoursOfOperation { get; set; }

        [StringLength(200)]
        [Display(Name = "Maintenance window")]
        public string MaintenanceWindow { get; set; }

        [StringLength(200)]
        [EmailAddress(ErrorMessage = "Maintenance notification email is not a valid email address.")]
        [Display(Name = "Maintenance notification email")]
        public string MaintenanceNotificationEmail { get; set; }

        [StringLength(2000)]
        [DataType(DataType.MultilineText)]
        [Display(Name = "Application summary")]
        public string ApplicationSummary { get; set; }

        [StringLength(2000)]
        [DataType(DataType.MultilineText)]
        [Display(Name = "Triage steps")]
        public string TriageSteps { get; set; }

        public IEnumerable<SelectListItem> EnvironmentOptions { get; set; }
        public IEnumerable<SelectListItem> CriticalityOptions { get; set; }
        public IEnumerable<SelectListItem> StatusOptions { get; set; }
        public IEnumerable<SelectListItem> CategoryOptions { get; set; }
        public IEnumerable<SelectListItem> FamilyOptions { get; set; }
        public IEnumerable<SelectListItem> TeamOptions { get; set; }

        /// <summary>Ids of the technology tags applied to this application.</summary>
        public IEnumerable<int> SelectedTechnologyIds { get; set; }

        public IEnumerable<SelectListItem> TechnologyOptions { get; set; }

        /// <summary>Ids of the shared user tags applied to this application.</summary>
        public IEnumerable<int> SelectedTagIds { get; set; }

        public IEnumerable<SelectListItem> TagOptions { get; set; }

        /// <summary>
        /// Comma-separated names typed by the user; each is created in the shared
        /// tag pool (reused by name, case-insensitively) when the application saves.
        /// </summary>
        [Display(Name = "New tags")]
        public string NewTagNames { get; set; }

        public IList<ApplicationPropertyViewModel> Properties { get; set; }
    }

    public class ApplicationDetailViewModel
    {
        public Application Application { get; set; }
        public IEnumerable<DependencyListEntryViewModel> UpstreamDependencies { get; set; }
        public IEnumerable<DependencyListEntryViewModel> DownstreamDependencies { get; set; }
        public bool CanEdit { get; set; }
        public IEnumerable<ApplicationPropertyViewModel> Properties { get; set; }
        public IEnumerable<ApplicationDllViewModel> Dlls { get; set; }

        /// <summary>Technology tags applied to this application.</summary>
        public IEnumerable<ApplicationTechnology> Technologies { get; set; }

        /// <summary>Shared user tags applied to this application.</summary>
        public IEnumerable<ApplicationTag> Tags { get; set; }

        /// <summary>Other deployed versions of the same application name (excluding the current row).</summary>
        public IEnumerable<VersionLinkViewModel> OtherVersions { get; set; }

        /// <summary>True when the application can be permanently deleted (no dependencies).</summary>
        public bool CanDelete { get; set; }

        public int DependencyCount { get; set; }
    }

    /// <summary>A link to another deployed version of the same application.</summary>
    public class VersionLinkViewModel
    {
        public int ApplicationId { get; set; }
        public string Version { get; set; }
    }

    /// <summary>One library (DLL) used by an application, with its deployed version.</summary>
    public class ApplicationDllViewModel
    {
        public string FileName { get; set; }
        public string Version { get; set; }
        public string RelativePath { get; set; }
    }

    /// <summary>
    /// A single row in an application's upstream (depends-on) or downstream
    /// (depended-on) list. Includes both direct (Depth 1) and indirect
    /// (transitive, Depth &gt; 1) entries.
    /// </summary>
    public class DependencyListEntryViewModel
    {
        public int ApplicationId { get; set; }
        public string Name { get; set; }

        /// <summary>The neighbor application's type (category), e.g. Internal, External, Database.</summary>
        public string Category { get; set; }

        public int Depth { get; set; }
        public bool IsDirect { get; set; }
        public string DependencyTypes { get; set; }
        public string CriticalityLevel { get; set; }
        public string Impact { get; set; }
        public string Frequency { get; set; }
        public int DependencyId { get; set; }
        /// <summary>
        /// The dependency chain from the viewed application to this indirect entry,
        /// rendered as links. Null for direct (depth 1) entries.
        /// </summary>
        public IEnumerable<DependencyPathStepViewModel> Chain { get; set; }
    }

    /// <summary>
    /// One application in the chain that connects an indirect dependency back to
    /// the application being viewed.
    /// </summary>
    public class DependencyPathStepViewModel
    {
        public int ApplicationId { get; set; }
        public string Name { get; set; }
    }

    public class ApplicationPropertyViewModel
    {
        public int PropertyDefinitionId { get; set; }
        public string Key { get; set; }
        public string Label { get; set; }
        public string DataType { get; set; }
        public string Value { get; set; }
        public string ScanPattern { get; set; }
    }

    public class DeleteApplicationViewModel
    {
        public int ApplicationId { get; set; }
        public string Name { get; set; }
        public string Environment { get; set; }
        public string CriticalityLevel { get; set; }
        public int DependencyCount { get; set; }
        public bool CanDelete { get; set; }

        /// <summary>True when the application is already soft-deleted.</summary>
        public bool IsDeleted { get; set; }
    }

    public static class ApplicationViewModelFactory
    {
        public static readonly string[] Environments = { "Production", "Staging", "Development" };
        public static readonly string[] Criticalities = { "Critical", "High", "Medium", "Low" };
        public static readonly string[] Statuses = { "Active", "Planned", "Retired" };

        public static IEnumerable<SelectListItem> EnvironmentOptions(string selected)
        {
            foreach (var e in Environments)
                yield return new SelectListItem { Text = e, Value = e, Selected = e == selected };
        }

        public static IEnumerable<SelectListItem> CriticalityOptions(string selected)
        {
            foreach (var c in Criticalities)
                yield return new SelectListItem { Text = c, Value = c, Selected = c == selected };
        }

        public static IEnumerable<SelectListItem> StatusOptions(string selected)
        {
            foreach (var s in Statuses)
                yield return new SelectListItem { Text = s, Value = s, Selected = s == selected };
        }
    }
}
