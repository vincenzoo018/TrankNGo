using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TrackNGo.Models
{
    /// <summary>
    /// Digital signature record for document approval and authentication (Module 5).
    /// Stores the signature image, hash for verification, and signer identity.
    /// </summary>
    public class DigitalSignature
    {
        public int Id { get; set; }

        public int DocumentId { get; set; }
        [ForeignKey("DocumentId")]
        public Document? Document { get; set; }

        public int SignedByUserId { get; set; }
        [ForeignKey("SignedByUserId")]
        public User? SignedBy { get; set; }

        /// <summary>Path to the signature image file (PNG from canvas capture)</summary>
        [MaxLength(500)]
        public string SignatureImagePath { get; set; } = string.Empty;

        /// <summary>SHA-256 hash of the signature for verification integrity</summary>
        [MaxLength(128)]
        public string SignatureHash { get; set; } = string.Empty;

        /// <summary>Type of action: "Approved", "Reviewed", "Endorsed"</summary>
        [Required, MaxLength(50)]
        public string ActionType { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Remarks { get; set; }

        public DateTime SignedAt { get; set; } = DateTime.UtcNow;

        public bool IsVerified { get; set; } = true;
    }
}
