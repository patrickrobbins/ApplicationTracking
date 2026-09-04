using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DependencyTracker.Data.Models
{
    /// <summary>
    /// An application tracked in the dependency registry.
    /// </summary>
    [Table("Applications")]
    public class Application
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ApplicationId { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; }

        /// <summary>
        /// Deployed version of this application. Multiple versions of the same
        /// application are tracked as separate rows; (Name, Version) is unique.
        /// </summary>
        [StringLength(50)]
        public string Version { get; set; }

        [StringLength(2000)]
        public string Description { get; set; }

        [StringLength(200)]
        public string BusinessGroup { get; set; }

        [StringLength(200)]
        public string BusinessOwner { get; set; }

        [StringLength(200)]
        public string BusinessOwnerEmail { get; set; }

        [StringLength(200)]
        public string BusinessBackup { get; set; }

        [StringLength(200)]
        public string BusinessBackupEmail { get; set; }

        [StringLength(200)]
        public string TechnicalOwnerEmail { get; set; }

        [StringLength(50)]
        public string Environment { get; set; }

        [StringLength(20)]
        public string CriticalityLevel { get; set; }

        /// <summary>
        /// Default criticality applied to new dependencies that target this
        /// application (this app is the provider). Pre-fills the create form.
        /// </summary>
        [StringLength(20)]
        public string DefaultDependencyCriticality { get; set; }

        /// <summary>
        /// Default impact note applied to new dependencies that target this
        /// application (this app is the provider). Pre-fills the create form.
        /// </summary>
        [StringLength(2000)]
        public string DefaultDependencyImpact { get; set; }

        [StringLength(20)]
        public string Status { get; set; }

        [StringLength(500)]
        public string ExternalUrl { get; set; }

        /// <summary>
        /// Physical directory (local path or UNC share) this application was
        /// discovered from by the IIS/network scanner. Used as the stable key
        /// for matching updates on re-scan.
        /// </summary>
        [StringLength(500)]
        public string SourcePath { get; set; }

        /// <summary>Development IDE used to build the application.</summary>
        [StringLength(200)]
        public string ApplicationIDE { get; set; }

        /// <summary>Runtime framework / .NET version the application runs on.</summary>
        [StringLength(100)]
        public string FrameworkVersion { get; set; }

        /// <summary>URL of the application documentation.</summary>
        [StringLength(500)]
        public string DocumentationLink { get; set; }

        /// <summary>Repository URL / source control location.</summary>
        [StringLength(500)]
        public string SourceControlLocation { get; set; }

        /// <summary>Supported operating hours, e.g. 24x7.</summary>
        [StringLength(200)]
        public string HoursOfOperation { get; set; }

        /// <summary>Scheduled maintenance window, e.g. Sat 02:00-04:00.</summary>
        [StringLength(200)]
        public string MaintenanceWindow { get; set; }

        /// <summary>Email notified of planned maintenance.</summary>
        [StringLength(200)]
        public string MaintenanceNotificationEmail { get; set; }

        /// <summary>Short summary of what the application does.</summary>
        [StringLength(2000)]
        public string ApplicationSummary { get; set; }

        /// <summary>Incident triage / runbook steps.</summary>
        [StringLength(2000)]
        public string TriageSteps { get; set; }

        public int? CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        public virtual ApplicationCategory Category { get; set; }

        public int? FamilyId { get; set; }

        [ForeignKey("FamilyId")]
        public virtual ApplicationFamily Family { get; set; }

        public int? TechnicalOwnershipTeamId { get; set; }

        [ForeignKey("TechnicalOwnershipTeamId")]
        public virtual TechnicalOwnershipTeam TechnicalOwnershipTeam { get; set; }

        /// <summary>
        /// Soft-delete flag. Deleted applications are hidden from the registry,
        /// graph and discovery but kept in the database so they can be restored.
        /// </summary>
        public bool IsDeleted { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime ModifiedDate { get; set; }

        [StringLength(128)]
        public string CreatedBy { get; set; }

        [StringLength(128)]
        public string ModifiedBy { get; set; }

        [InverseProperty("SourceApplication")]
        public virtual ICollection<Dependency> OutgoingDependencies { get; set; }

        [InverseProperty("TargetApplication")]
        public virtual ICollection<Dependency> IncomingDependencies { get; set; }

        [InverseProperty("Application")]
        public virtual ICollection<ApplicationDll> Dlls { get; set; }

        [InverseProperty("Application")]
        public virtual ICollection<ApplicationTechnologyMapping> TechnologyMappings { get; set; }

        [InverseProperty("Application")]
        public virtual ICollection<ApplicationTagMapping> TagMappings { get; set; }

        public Application()
        {
            OutgoingDependencies = new HashSet<Dependency>();
            IncomingDependencies = new HashSet<Dependency>();
            Dlls = new HashSet<ApplicationDll>();
            TechnologyMappings = new HashSet<ApplicationTechnologyMapping>();
            TagMappings = new HashSet<ApplicationTagMapping>();
        }
    }
}
