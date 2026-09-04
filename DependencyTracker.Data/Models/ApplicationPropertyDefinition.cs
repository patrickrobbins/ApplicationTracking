using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DependencyTracker.Data.Models
{
    /// <summary>
    /// A managed definition of a dynamic per-application property. The optional
    /// ScanPattern is a regular expression matched against configuration keys and
    /// values by the configuration scanner.
    /// </summary>
    [Table("ApplicationPropertyDefinitions")]
    public class ApplicationPropertyDefinition
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PropertyDefinitionId { get; set; }

        [Required]
        [StringLength(100)]
        public string Key { get; set; }

        [Required]
        [StringLength(200)]
        public string Label { get; set; }

        [Required]
        [StringLength(20)]
        public string DataType { get; set; }

        [StringLength(500)]
        public string ScanPattern { get; set; }

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
