using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TrackNGo.Models
{
    /// <summary>
    /// ARTA Escalation log (Table 17).
    /// Records when and why a document was escalated for exceeding processing deadlines.
    /// Supports 3-tier escalation: Warning (3 days), Critical (7 days), Overdue (20 days).
    /// </summary>
    public class EscalationLog
    {
        public int Id { get; set; }

        public int DocumentId { get; set; }
        [ForeignKey("DocumentId")]
        public Document? Document { get; set; }

        /// <summary>The office that failed to process the document within the ARTA period</summary>
        [Required, MaxLength(100)]
        public string ViolatingOffice { get; set; } = string.Empty;

        /// <summary>ARTA processing day limit breached: 3, 7, or 20 working days</summary>
        public int ARTAThreshold { get; set; }

        /// <summary>ARTA processing period that was exceeded: 3, 7, or 20 working days (legacy)</summary>
        public int ARTAPeriodDays { get; set; }

        /// <summary>Actual number of working days elapsed when escalation was triggered</summary>
        public int ActualElapsedDays { get; set; }

        /// <summary>Severity level: Warning, Critical, Overdue</summary>
        [Required, MaxLength(20)]
        public string EscalationLevel { get; set; } = "Warning";

        /// <summary>Oversight or compliance officer notified of the escalation</summary>
        public int? NotifiedUserId { get; set; }
        [ForeignKey("NotifiedUserId")]
        public User? NotifiedUser { get; set; }

        /// <summary>Whether the alert notification was sent: true = Sent, false = Pending</summary>
        public bool NotificationSent { get; set; } = false;

        [Required, MaxLength(500)]
        public string EscalationReason { get; set; } = string.Empty;

        public DateTime EscalatedAt { get; set; } = DateTime.UtcNow;

        [MaxLength(1000)]
        public string? ResolutionNotes { get; set; }

        public DateTime? ResolvedAt { get; set; }

        /// <summary>Whether the escalation has been resolved</summary>
        public bool Resolved { get; set; } = false;

        public int? ResolvedByUserId { get; set; }
        [ForeignKey("ResolvedByUserId")]
        public User? ResolvedBy { get; set; }
    }
}
