using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DependencyTracker.Data.Models
{
    /// <summary>
    /// Maps an Active Directory group to an application role.
    /// </summary>
    [Table("ADGroups")]
    public class AdGroup
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int GroupId { get; set; }

        [Required]
        [StringLength(200)]
        [Index(IsUnique = true)]
        public string GroupName { get; set; }

        [Required]
        [StringLength(50)]
        public string Role { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedDate { get; set; }
    }
}
