using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TrackNGo.Models
{
    /// <summary>
    /// Audit log for password-gated document exports (Module 7).
    /// Records every export action for accountability.
    /// </summary>
    public class ExportAuditLog
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public User? User { get; set; }

        /// <summary>Export format: PDF, CSV, Excel</summary>
        [Required, MaxLength(20)]
        public string ExportType { get; set; } = string.Empty;

        /// <summary>Description of what was exported: "All Documents", "Filtered: May 2026", etc.</summary>
        [Required, MaxLength(200)]
        public string ExportScope { get; set; } = string.Empty;

        public int RecordCount { get; set; }

        public DateTime ExportedAt { get; set; } = DateTime.UtcNow;

        [MaxLength(50)]
        public string? IPAddress { get; set; }
    }
}
