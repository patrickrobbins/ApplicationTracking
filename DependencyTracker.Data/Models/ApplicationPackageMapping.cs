using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DependencyTracker.Data.Models
{
    /// <summary>
    /// Join between an application and a shared package, recording the specific
    /// version of the package deployed by that application. Deleting either
    /// side removes the mapping (cascade in the database and via EF config).
    /// </summary>
    [Table("ApplicationPackageMappings")]
    public class ApplicationPackageMapping
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MappingId { get; set; }

        public int ApplicationId { get; set; }

        public int PackageId { get; set; }

        [StringLength(100)]
        public string Version { get; set; }

        [ForeignKey("ApplicationId")]
        public virtual Application Application { get; set; }

        [ForeignKey("PackageId")]
        public virtual Package Package { get; set; }
    }
}