using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TrackNGo.Models
{
    /// <summary>
    /// SMS notification record for automated messaging (Table 14).
    /// Tracks each SMS sent, its delivery status, and the triggering event.
    /// </summary>
    public class SMSNotification
    {
        public int Id { get; set; }

        public int DocumentId { get; set; }
        [ForeignKey("DocumentId")]
        public Document? Document { get; set; }

        /// <summary>FK to Users — recipient user (Table 14 alignment).</summary>
        public int? RecipientUserId { get; set; }
        [ForeignKey("RecipientUserId")]
        public User? RecipientUser { get; set; }

        [Required, MaxLength(20)]
        public string RecipientNumber { get; set; } = string.Empty;

        [MaxLength(100)]
        public string RecipientName { get; set; } = string.Empty;

        [Required, MaxLength(500)]
        public string MessageContent { get; set; } = string.Empty;

        /// <summary>Event that triggered this SMS: StatusChange, Approval, Escalation, FeedbackRequest, etc.</summary>
        [Required, MaxLength(50)]
        public string TriggerEvent { get; set; } = string.Empty;

        public SMSStatus Status { get; set; } = SMSStatus.Queued;

        public DateTime QueuedAt { get; set; } = DateTime.UtcNow;

        public DateTime? SentAt { get; set; }

        [MaxLength(500)]
        public string? GatewayResponse { get; set; }
    }
}
