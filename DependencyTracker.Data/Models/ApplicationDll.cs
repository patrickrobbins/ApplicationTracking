using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DependencyTracker.Data.Models
{
    /// <summary>
    /// A library (DLL) an application uses, with the specific version deployed,
    /// discovered from the application's source path (directory and subdirectories).
    /// </summary>
    [Table("ApplicationDlls")]
    public class ApplicationDll
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ApplicationDllId { get; set; }

        public int ApplicationId { get; set; }

        [ForeignKey("ApplicationId")]
        public virtual Application Application { get; set; }

        /// <summary>Assembly / file name (e.g. Newtonsoft.Json.dll).</summary>
        [Required]
        [StringLength(260)]
        public string FileName { get; set; }

        /// <summary>Deployed file version, e.g. 13.0.3.27908. Null when none was readable.</summary>
        [StringLength(100)]
        public string Version { get; set; }

        /// <summary>Path relative to the application source path (e.g. bin\Newtonsoft.Json.dll).</summary>
        [StringLength(1000)]
        public string RelativePath { get; set; }

        public DateTime ModifiedDate { get; set; }

        [StringLength(128)]
        public string ModifiedBy { get; set; }
    }
}
