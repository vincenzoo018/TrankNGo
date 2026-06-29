using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TrackNGo.Models
{
    /// <summary>
    /// Core document entity. Represents a document in the TrackNGo system
    /// with FSM state tracking, ARTA compliance fields, and relationships
    /// to workflow transitions, comments, signatures, and audit trail.
    /// </summary>
    public class Document
    {
        public int Id { get; set; }

        /// <summary>Auto-generated reference number, format: TNG-YYYY-NNNN (reference_number in spec)</summary>
        [Required, MaxLength(30)]
        public string TrackingNumber { get; set; } = string.Empty;

        /// <summary>Subject or title of the document</summary>
        [Required, MaxLength(300)]
        public string Title { get; set; } = string.Empty;

        /// <summary>Document category string (kept for backward compat)</summary>
        [Required, MaxLength(50)]
        public string DocumentType { get; set; } = string.Empty;

        /// <summary>FK to DocumentTypeConfig (Table 4 alignment)</summary>
        public int? TypeId { get; set; }
        [ForeignKey("TypeId")]
        public DocumentTypeConfig? DocumentTypeConfig { get; set; }

        /// <summary>The department name that originated this document (kept for backward compat)</summary>
        [Required, MaxLength(100)]
        public string OriginatingDepartment { get; set; } = string.Empty;

        /// <summary>FK to Departments table (Table 5 alignment)</summary>
        public int? DepartmentId { get; set; }
        [ForeignKey("DepartmentId")]
        public Department? DepartmentEntity { get; set; }

        /// <summary>Name of the person who submitted the document (kept for backward compat)</summary>
        [Required, MaxLength(100)]
        public string SubmittedBy { get; set; } = string.Empty;

        /// <summary>FK to Users table — user who submitted (Table 5 alignment)</summary>
        public int? SubmittedByUserId { get; set; }
        [ForeignKey("SubmittedByUserId")]
        public User? SubmittedByUser { get; set; }

        /// <summary>Contact number for SMS notifications (Module 4)</summary>
        [MaxLength(20)]
        public string? ContactNumber { get; set; }

        [MaxLength(100)]
        public string? EmailAddress { get; set; }

        // ── FSM State (Module 2) ──

        /// <summary>Current document status in the FSM lifecycle</summary>
        public DocumentStatus CurrentStatus { get; set; } = DocumentStatus.Submitted;

        /// <summary>Name of the office currently holding/processing the document</summary>
        [MaxLength(100)]
        public string CurrentOfficeName { get; set; } = string.Empty;

        /// <summary>Current step index in the workflow (e.g. 3 of 7)</summary>
        public int CurrentStepIndex { get; set; } = 1;

        /// <summary>Total number of steps in this document's workflow</summary>
        public int TotalSteps { get; set; } = 5;

        // ── File Management ──

        /// <summary>Legacy: Path to single uploaded attachment (kept for backward compat)</summary>
        [MaxLength(500)]
        public string? AttachmentPath { get; set; }

        /// <summary>Path to the generated QR code image (Module 3)</summary>
        [MaxLength(500)]
        public string? QRCodePath { get; set; }

        // ── Timestamps ──

        public DateTime DateFiled { get; set; } = DateTime.UtcNow;

        public DateTime? DateCompleted { get; set; }

        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

        // ── ARTA Compliance (Module 6 — Capstone Element) ──

        /// <summary>ARTA processing period in working days: 3, 7, or 20</summary>
        public int ARTAProcessingDays { get; set; } = 3;

        /// <summary>Calculated deadline based on ARTA period</summary>
        public DateTime? EscalationDeadline { get; set; }

        /// <summary>Whether this document has been auto-escalated</summary>
        public bool IsEscalated { get; set; } = false;

        /// <summary>Whether this document is locked from further action (post-escalation)</summary>
        public bool IsLocked { get; set; } = false;

        // ── Relationships ──

        public int CreatedByUserId { get; set; }
        [ForeignKey("CreatedByUserId")]
        public User? CreatedBy { get; set; }

        public ICollection<WorkflowTransition> Transitions { get; set; } = new List<WorkflowTransition>();
        public ICollection<DocumentComment> Comments { get; set; } = new List<DocumentComment>();
        public ICollection<DigitalSignature> Signatures { get; set; } = new List<DigitalSignature>();
        public ICollection<AuditTrailEntry> AuditTrail { get; set; } = new List<AuditTrailEntry>();
        public ICollection<SMSNotification> Notifications { get; set; } = new List<SMSNotification>();
        public ICollection<DocumentAttachment> Attachments { get; set; } = new List<DocumentAttachment>();

        // One-to-one relationships
        public DocumentMetadata? Metadata { get; set; }
        public RoutingSlip? RoutingSlip { get; set; }
    }
}
