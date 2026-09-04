using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DependencyTracker.Data.Models
{
    /// <summary>
    /// Join between an application and a shared tag. Deleting either side
    /// removes the mapping (cascade in the database and via EF configuration).
    /// </summary>
    [Table("ApplicationTagMappings")]
    public class ApplicationTagMapping
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MappingId { get; set; }

        public int ApplicationId { get; set; }

        public int TagId { get; set; }

        [ForeignKey("ApplicationId")]
        public virtual Application Application { get; set; }

        [ForeignKey("TagId")]
        public virtual ApplicationTag Tag { get; set; }
    }
}