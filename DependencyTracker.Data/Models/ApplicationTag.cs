using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DependencyTracker.Data.Models
{
    /// <summary>
    /// A shared, user-generated tag applied to applications. Tags are created
    /// on-the-fly from the application form as well as through the
    /// Administration area; every tag lives in one shared pool and is reused
    /// across applications.
    /// </summary>
    [Table("ApplicationTags")]
    public class ApplicationTag
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TagId { get; set; }

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

        [InverseProperty("Tag")]
        public virtual ICollection<ApplicationTagMapping> Mappings { get; set; }

        public ApplicationTag()
        {
            Mappings = new HashSet<ApplicationTagMapping>();
        }
    }
}