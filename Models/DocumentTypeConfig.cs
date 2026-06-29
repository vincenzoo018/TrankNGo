using System.ComponentModel.DataAnnotations;

namespace TrackNGo.Models
{
    /// <summary>
    /// Document Type configuration entity (Table 4).
    /// Defines available document types and their total workflow steps.
    /// </summary>
    public class DocumentTypeConfig
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string TypeName { get; set; } = string.Empty;

        /// <summary>Total number of workflow steps for this document type.</summary>
        public int TotalSteps { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        // Navigation
        public ICollection<WorkflowStep> WorkflowSteps { get; set; } = new List<WorkflowStep>();
    }
}
