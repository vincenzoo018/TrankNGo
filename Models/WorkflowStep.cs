using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TrackNGo.Models
{
    /// <summary>
    /// Workflow Step configuration entity (Table 8).
    /// Defines the steps in the FSM workflow for each document type.
    /// Configurable by Executive Administrator.
    /// </summary>
    public class WorkflowStep
    {
        [Key]
        public int Id { get; set; }

        public int TypeId { get; set; }
        [ForeignKey("TypeId")]
        public DocumentTypeConfig? DocumentType { get; set; }

        /// <summary>Order of the step in the workflow (1-based).</summary>
        public int StepNumber { get; set; }

        [Required, MaxLength(150)]
        public string StepName { get; set; } = string.Empty;

        public int? AssignedDepartmentId { get; set; }
        [ForeignKey("AssignedDepartmentId")]
        public Department? AssignedDepartment { get; set; }

        /// <summary>Allowed actions at this step (e.g. "Forward, Approve, Return, Reject").</summary>
        [Required, MaxLength(200)]
        public string AllowedActions { get; set; } = string.Empty;
    }
}
