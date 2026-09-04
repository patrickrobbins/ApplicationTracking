using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DependencyTracker.Data.Models
{
    /// <summary>
    /// A dependency relationship. SourceApplicationId DEPENDS ON TargetApplicationId.
    /// </summary>
    [Table("Dependencies")]
    public class Dependency
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int DependencyId { get; set; }

        public int SourceApplicationId { get; set; }

        public int TargetApplicationId { get; set; }

        [Required]
        [StringLength(50)]
        public string DependencyType { get; set; }

        [StringLength(20)]
        public string Direction { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [StringLength(20)]
        public string CriticalityLevel { get; set; }

        [StringLength(2000)]
        public string Impact { get; set; }

        [StringLength(50)]
        public string Frequency { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime ModifiedDate { get; set; }

        [StringLength(128)]
        public string CreatedBy { get; set; }

        [StringLength(128)]
        public string ModifiedBy { get; set; }

        [ForeignKey("SourceApplicationId")]
        public virtual Application SourceApplication { get; set; }

        [ForeignKey("TargetApplicationId")]
        public virtual Application TargetApplication { get; set; }
    }
}
