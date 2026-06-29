using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TrackNGo.Models
{
    /// <summary>
    /// Routing Slip entity (Table 16).
    /// Physical routing slip attached to each document as it moves through the workflow.
    /// </summary>
    public class RoutingSlip
    {
        [Key]
        public int Id { get; set; }

        public int DocumentId { get; set; }
        [ForeignKey("DocumentId")]
        public Document? Document { get; set; }

        /// <summary>System-generated tracking number assigned upon document receipt.</summary>
        [Required, MaxLength(30)]
        public string TrackingNumber { get; set; } = string.Empty;

        /// <summary>Receiving clerk who accepted and recorded the document.</summary>
        public int ReceivedByUserId { get; set; }
        [ForeignKey("ReceivedByUserId")]
        public User? ReceivedBy { get; set; }

        public DateTime DateReceived { get; set; } = DateTime.UtcNow;

        /// <summary>Name of the client or originating office that submitted the document.</summary>
        [Required, MaxLength(150)]
        public string SenderName { get; set; } = string.Empty;

        /// <summary>Instructions noted on the routing slip (e.g. For Signature, For Review).</summary>
        [MaxLength(1000)]
        public string? ActionInstruction { get; set; }

        public int? TargetDepartmentId { get; set; }
        [ForeignKey("TargetDepartmentId")]
        public Department? TargetDepartment { get; set; }

        /// <summary>Mayor or City Administrator who noted the routing slip.</summary>
        public int? NotedByUserId { get; set; }
        [ForeignKey("NotedByUserId")]
        public User? NotedBy { get; set; }

        /// <summary>Current status of the slip: Active, Completed, Returned.</summary>
        [Required, MaxLength(30)]
        public string SlipStatus { get; set; } = "Active";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
