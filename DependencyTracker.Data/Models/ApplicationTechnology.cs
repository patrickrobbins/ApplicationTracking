using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DependencyTracker.Data.Models
{
    /// <summary>
    /// A managed application technology tag (e.g. .NET Framework, ASP.NET,
    /// SQL Server, Angular). The list is dynamic and user-maintained; tags are
    /// searchable and filterable.
    /// </summary>
    [Table("ApplicationTechnologies")]
    public class ApplicationTechnology
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TechnologyId { get; set; }

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

        [InverseProperty("Technology")]
        public virtual ICollection<ApplicationTechnologyMapping> Mappings { get; set; }

        public ApplicationTechnology()
        {
            Mappings = new HashSet<ApplicationTechnologyMapping>();
        }
    }
}
