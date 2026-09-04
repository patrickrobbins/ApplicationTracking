using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DependencyTracker.Data.Models
{
    /// <summary>
    /// A managed application family (e.g. Business Applications, Shared
    /// Services, Data &amp; Analytics). The list is dynamic and maintained by
    /// administrators; each application belongs to at most one family.
    /// </summary>
    [Table("ApplicationFamilies")]
    public class ApplicationFamily
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int FamilyId { get; set; }

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
    }
}