using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TrackNGo.Models
{
    /// <summary>
    /// Records each FSM state transition in the document lifecycle (Module 2).
    /// Tracks who performed the action, from/to status, and the office involved.
    /// </summary>
    public class WorkflowTransition
    {
        public int Id { get; set; }

        public int DocumentId { get; set; }
        [ForeignKey("DocumentId")]
        public Document? Document { get; set; }

        public DocumentStatus FromStatus { get; set; }
        public DocumentStatus ToStatus { get; set; }

        [MaxLength(100)]
        public string FromOffice { get; set; } = string.Empty;

        [MaxLength(100)]
        public string ToOffice { get; set; } = string.Empty;

        /// <summary>Action description: "Reviewed", "Approved", "Returned", "Escalated", etc.</summary>
        [Required, MaxLength(50)]
        public string ActionTaken { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Remarks { get; set; }

        public int PerformedByUserId { get; set; }
        [ForeignKey("PerformedByUserId")]
        public User? PerformedBy { get; set; }

        public DateTime TransitionDate { get; set; } = DateTime.UtcNow;

        /// <summary>Which step in the workflow sequence this transition represents</summary>
        public int StepNumber { get; set; }
    }
}
