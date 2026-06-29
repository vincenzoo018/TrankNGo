using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TrackNGo.Models
{
    /// <summary>
    /// QR code record linked to a document for tracking purposes (Module 3).
    /// Stores the generated QR code image path, tracking URL, and scan metrics.
    /// </summary>
    public class QRCodeRecord
    {
        public int Id { get; set; }

        public int DocumentId { get; set; }
        [ForeignKey("DocumentId")]
        public Document? Document { get; set; }

        [Required, MaxLength(20)]
        public string TrackingNumber { get; set; } = string.Empty;

        [MaxLength(500)]
        public string QRCodeImagePath { get; set; } = string.Empty;

        [MaxLength(500)]
        public string TrackingUrl { get; set; } = string.Empty;

        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

        public int ScanCount { get; set; } = 0;

        public DateTime? LastScannedAt { get; set; }
    }
}
