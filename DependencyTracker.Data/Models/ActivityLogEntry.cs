using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DependencyTracker.Data.Models
{
    /// <summary>
    /// Audit trail entry for every create/update/delete operation.
    /// </summary>
    [Table("ActivityLog")]
    public class ActivityLogEntry
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int LogId { get; set; }

        [Required]
        [StringLength(50)]
        public string Action { get; set; }

        [Required]
        [StringLength(50)]
        public string EntityType { get; set; }

        public int EntityId { get; set; }

        [StringLength(200)]
        public string EntityName { get; set; }

        [StringLength(2000)]
        public string Details { get; set; }

        [Required]
        [StringLength(128)]
        public string PerformedBy { get; set; }

        public DateTime PerformedDate { get; set; }
    }
}
