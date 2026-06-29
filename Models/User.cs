using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TrackNGo.Models
{
    /// <summary>
    /// Represents a system user with role-based access.
    /// Roles: Mayor, ExecutiveAdmin, RecordsOfficer, OversightOfficer
    /// </summary>
    public class User
    {
        public int Id { get; set; }

        [Required, MaxLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Email { get; set; }

        [MaxLength(20)]
        public string? PhoneNumber { get; set; }

        /// <summary>Mobile number for SMS notifications (Table 2).</summary>
        [MaxLength(20)]
        public string? MobileNumber { get; set; }

        public UserRole Role { get; set; }

        /// <summary>Department name (kept for backward compatibility).</summary>
        [MaxLength(100)]
        public string Department { get; set; } = string.Empty;

        /// <summary>FK to Departments table (Table 2 alignment).</summary>
        public int? DepartmentId { get; set; }
        [ForeignKey("DepartmentId")]
        public Department? DepartmentEntity { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? LastLoginAt { get; set; }

        [MaxLength(250)]
        public string? ProfileImagePath { get; set; }

        // Secondary password for password-gated document export (Module 7)
        public string? ExportPasswordHash { get; set; }
    }
}
