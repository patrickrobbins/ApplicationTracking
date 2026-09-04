using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DependencyTracker.Data.Models
{
    /// <summary>
    /// Join between an application and a technology tag. Deleting either side
    /// removes the mapping (cascade in the database and via EF configuration).
    /// </summary>
    [Table("ApplicationTechnologyMappings")]
    public class ApplicationTechnologyMapping
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MappingId { get; set; }

        public int ApplicationId { get; set; }

        public int TechnologyId { get; set; }

        [ForeignKey("ApplicationId")]
        public virtual Application Application { get; set; }

        [ForeignKey("TechnologyId")]
        public virtual ApplicationTechnology Technology { get; set; }
    }
}
