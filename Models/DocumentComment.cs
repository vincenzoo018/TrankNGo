using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TrackNGo.Models
{
    /// <summary>
    /// Remarks/Comments attached to documents during workflow processing (Table 10).
    /// Supports typed remarks: Comment, Return Remark, Approval Note.
    /// </summary>
    public class DocumentComment
    {
        public int Id { get; set; }

        public int DocumentId { get; set; }
        [ForeignKey("DocumentId")]
        public Document? Document { get; set; }

        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public User? User { get; set; }

        [Required, MaxLength(2000)]
        public string Content { get; set; } = string.Empty;

        public DateTime PostedAt { get; set; } = DateTime.UtcNow;

        /// <summary>If true, only visible to internal staff; if false, visible to clients too.</summary>
        public bool IsInternal { get; set; } = true;

        /// <summary>Remark type: Comment, Return Remark, Approval Note (Table 10 alignment).</summary>
        [MaxLength(20)]
        public string RemarkType { get; set; } = "Comment";
    }
}
