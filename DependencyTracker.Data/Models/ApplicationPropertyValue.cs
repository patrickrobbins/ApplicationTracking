using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DependencyTracker.Data.Models
{
    /// <summary>
    /// A per-application value for a property definition.
    /// </summary>
    [Table("ApplicationPropertyValues")]
    public class ApplicationPropertyValue
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PropertyValueId { get; set; }

        public int ApplicationId { get; set; }

        public int PropertyDefinitionId { get; set; }

        [StringLength(2000)]
        public string Value { get; set; }

        public DateTime ModifiedDate { get; set; }

        [StringLength(128)]
        public string ModifiedBy { get; set; }

        [ForeignKey("ApplicationId")]
        public virtual Application Application { get; set; }

        [ForeignKey("PropertyDefinitionId")]
        public virtual ApplicationPropertyDefinition Definition { get; set; }
    }
}
