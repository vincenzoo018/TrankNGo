using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TrackNGo.Models
{
    /// <summary>
    /// Document Metadata entity (Table 7).
    /// Stores extended metadata for documents (e.g. conference name for Research Incentive type).
    /// </summary>
    public class DocumentMetadata
    {
        [Key]
        public int Id { get; set; }

        public int DocumentId { get; set; }
        [ForeignKey("DocumentId")]
        public Document? Document { get; set; }

        [MaxLength(300)]
        public string? ConferenceName { get; set; }

        [MaxLength(500)]
        public string? SourceLink { get; set; }

        [MaxLength(100)]
        public string? Province { get; set; }

        [MaxLength(50)]
        public string? ReportNumber { get; set; }

        /// <summary>Category options selected (e.g. For Incentive, For Productivity)</summary>
        [MaxLength(100)]
        public string? CategoryFlags { get; set; }
    }
}
