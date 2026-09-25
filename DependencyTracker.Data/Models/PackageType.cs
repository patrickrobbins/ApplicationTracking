using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DependencyTracker.Data.Models
{
    /// <summary>
    /// A managed package ecosystem (e.g. Npm, NuGet). The list is dynamic and
    /// maintained by administrators; every package belongs to exactly one type.
    /// </summary>
    [Table("PackageTypes")]
    public class PackageType
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PackageTypeId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        public bool IsActive { get; set; }

        public int SortOrder { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime ModifiedDate { get; set; }

        [StringLength(128)]
        public string CreatedBy { get; set; }

        [StringLength(128)]
        public string ModifiedBy { get; set; }

        [InverseProperty("PackageType")]
        public virtual ICollection<Package> Packages { get; set; }

        public PackageType()
        {
            Packages = new HashSet<Package>();
        }
    }
}