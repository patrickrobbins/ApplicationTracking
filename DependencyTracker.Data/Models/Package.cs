using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DependencyTracker.Data.Models
{
    /// <summary>
    /// A shared package pool, keyed by (package type, name). Packages are
    /// created on-the-fly from the application form as well as through the
    /// Administration area; every package is reused across applications.
    /// The deployed version is tracked per application on the mapping table.
    /// </summary>
    [Table("Packages")]
    public class Package
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PackageId { get; set; }

        public int PackageTypeId { get; set; }

        [ForeignKey("PackageTypeId")]
        public virtual PackageType PackageType { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; }

        public bool IsActive { get; set; }

        public int SortOrder { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime ModifiedDate { get; set; }

        [StringLength(128)]
        public string CreatedBy { get; set; }

        [StringLength(128)]
        public string ModifiedBy { get; set; }

        [InverseProperty("Package")]
        public virtual ICollection<ApplicationPackageMapping> Mappings { get; set; }

        public Package()
        {
            Mappings = new HashSet<ApplicationPackageMapping>();
        }
    }
}