using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TrackNGo.Models
{
    /// <summary>
    /// Audit trail entry for tracking all system actions.
    /// Provides accountability and transparency for every operation.
    /// </summary>
    public class AuditTrailEntry
    {
        public int Id { get; set; }

        public int? DocumentId { get; set; }
        [ForeignKey("DocumentId")]
        public Document? Document { get; set; }

        public int? UserId { get; set; }
        [ForeignKey("UserId")]
        public User? User { get; set; }

        /// <summary>Action type: "Created", "StatusChanged", "Signed", "Exported", "Login", etc.</summary>
        [Required, MaxLength(50)]
        public string Action { get; set; } = string.Empty;

        /// <summary>Human-readable description of what happened</summary>
        [Required, MaxLength(1000)]
        public string Details { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? OldValue { get; set; }

        [MaxLength(500)]
        public string? NewValue { get; set; }

        [MaxLength(50)]
        public string? IPAddress { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
