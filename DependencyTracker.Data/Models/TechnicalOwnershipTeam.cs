using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DependencyTracker.Data.Models
{
    /// <summary>
    /// A managed team that owns the technical ownership of applications.
    /// Replaces the free-text technical-owner name on the application form;
    /// the application continues to store a free-text technical owner email.
    /// </summary>
    [Table("TechnicalOwnershipTeams")]
    public class TechnicalOwnershipTeam
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TeamId { get; set; }

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