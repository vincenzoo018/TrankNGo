using System.ComponentModel.DataAnnotations;
using TrackNGo.Models;

namespace TrackNGo.ViewModels
{
    // ── Authentication ──

    public class LoginViewModel
    {
        [Required(ErrorMessage = "Username is required")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; }
    }

    // ── Dashboard (Module 8) ──

    public class DashboardViewModel
    {
        public int TotalDocuments { get; set; }
        public int InProgressCount { get; set; }
        public int CompletedCount { get; set; }
        public int PendingCount { get; set; }
        public int EscalatedCount { get; set; }
        public int ReturnedCount { get; set; }

        // Role-specific counts
        public int PendingSignatureCount { get; set; }      // Mayor
        public int PendingApprovalCount { get; set; }        // Mayor
        public int ARTAFlaggedCount { get; set; }            // Mayor + Oversight
        public int NewlySubmittedCount { get; set; }         // ExecAdmin
        public int PendingMayorReviewCount { get; set; }     // ExecAdmin
        public int ReceivedTodayCount { get; set; }          // Records
        public int AwaitingResubmissionCount { get; set; }   // Records
        public int WarningCount { get; set; }                // Oversight
        public int CriticalCount { get; set; }               // Oversight
        public int OverdueCount { get; set; }                // Oversight

        public double ARTAComplianceRate { get; set; }       // Percentage
        public double AverageProcessingDays { get; set; }

        // Chart data
        public Dictionary<string, int> StatusDistribution { get; set; } = new();
        public Dictionary<string, int> DepartmentActivity { get; set; } = new();
        public Dictionary<string, double> DepartmentAvgDays { get; set; } = new();
        public List<MonthlyTrendItem> MonthlyTrend { get; set; } = new();

        // Recent activity
        public List<RecentActivityItem> RecentActivity { get; set; } = new();

        // Escalation alerts
        public List<EscalationAlertItem> EscalationAlerts { get; set; } = new();

        // Current user info
        public string UserFullName { get; set; } = string.Empty;
        public UserRole UserRole { get; set; }
    }

    public class MonthlyTrendItem
    {
        public string Month { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class RecentActivityItem
    {
        public string TrackingNumber { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string PerformedBy { get; set; } = string.Empty;
        public string TimeAgo { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }

    public class EscalationAlertItem
    {
        public int DocumentId { get; set; }
        public string TrackingNumber { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string ViolatingOffice { get; set; } = string.Empty;
        public int DaysOverdue { get; set; }
        public int ARTAPeriod { get; set; }
        public string EscalationLevel { get; set; } = "Warning";
        public DateTime EscalatedAt { get; set; }
    }

    // ── Document Management (Module 1) ──

    public class DocumentListViewModel
    {
        public List<DocumentListItem> Documents { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 12;
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

        // Filters
        public string? SearchQuery { get; set; }
        public string? FilterDocType { get; set; }
        public string? FilterDepartment { get; set; }
        public string? FilterStatus { get; set; }
        public string? FilterDateRange { get; set; }

        // Current user
        public UserRole UserRole { get; set; }
    }

    public class DocumentListItem
    {
        public int Id { get; set; }
        public string TrackingNumber { get; set; } = string.Empty;
        public string DocumentType { get; set; } = string.Empty;
        public string SubmittedBy { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public DateTime DateFiled { get; set; }
        public int CurrentStep { get; set; }
        public int TotalSteps { get; set; }
        public DocumentStatus Status { get; set; }
        public string StatusText => Status.ToString();
    }

    public class DocumentCreateViewModel
    {
        public string GeneratedTrackingNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Document type is required")]
        public string DocumentType { get; set; } = string.Empty;

        [Required(ErrorMessage = "Originating department is required")]
        public string OriginatingDepartment { get; set; } = string.Empty;

        [Required(ErrorMessage = "Subject/Title is required")]
        [MaxLength(300)]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Submitter name is required")]
        public string SubmittedBy { get; set; } = string.Empty;

        public string? ContactNumber { get; set; }

        public string? EmailAddress { get; set; }

        /// <summary>ARTA transaction type: Simple(3), Complex(7), HighlyTechnical(20)</summary>
        public int ARTAProcessingDays { get; set; } = 3;

        // ── Routing Slip Fields (Table 16) ──
        public string? ActionInstruction { get; set; }
        public int? TargetDepartmentId { get; set; }

        // ── Metadata Fields (Table 7) ──
        public string? ConferenceName { get; set; }
        public string? SourceLink { get; set; }
        public string? Province { get; set; }
        public string? ReportNumber { get; set; }
        public string? CategoryFlags { get; set; }

        // Available options for dropdowns (populated from DB)
        public List<string> DocumentTypes { get; set; } = new();
        public List<string> Departments { get; set; } = new();
        public List<DepartmentDropdownItem> DepartmentOptions { get; set; } = new();
    }

    public class DepartmentDropdownItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
    }

    public class DocumentDetailViewModel
    {
        public int Id { get; set; }
        public string TrackingNumber { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string DocumentType { get; set; } = string.Empty;
        public string OriginatingDepartment { get; set; } = string.Empty;
        public string SubmittedBy { get; set; } = string.Empty;
        public string? ContactNumber { get; set; }
        public DocumentStatus CurrentStatus { get; set; }
        public string CurrentOfficeName { get; set; } = string.Empty;
        public int CurrentStepIndex { get; set; }
        public int CurrentStep { get; set; }
        public int TotalSteps { get; set; }
        public DateTime DateFiled { get; set; }
        public DateTime? DateCompleted { get; set; }
        public DateTime LastUpdated { get; set; }
        public int ARTAProcessingDays { get; set; }
        public bool IsEscalated { get; set; }
        public bool IsLocked { get; set; }
        public string? AttachmentPath { get; set; }
        public string? QRCodePath { get; set; }

        // Related data
        public List<WorkflowTransition> Transitions { get; set; } = new();
        public List<DocumentComment> Comments { get; set; } = new();
        public List<DigitalSignature> Signatures { get; set; } = new();
        public List<DocumentAttachment> Attachments { get; set; } = new();

        // Routing slip
        public RoutingSlip? RoutingSlip { get; set; }

        // Metadata
        public DocumentMetadata? Metadata { get; set; }

        // User permissions for this document (RBAC)
        public bool CanApprove { get; set; }
        public bool CanReview { get; set; }
        public bool CanForward { get; set; }
        public bool CanReturn { get; set; }
        public bool CanSign { get; set; }
        public bool CanComment { get; set; }
        public bool CanEndorse { get; set; }
        public UserRole UserRole { get; set; }
    }

    // ── Workflow (Module 2) ──

    public class WorkflowActionViewModel
    {
        public int DocumentId { get; set; }
        public string TrackingNumber { get; set; } = string.Empty;

        [Required]
        public string Action { get; set; } = string.Empty; // "Forward", "Approve", "Return", "Reject", "Endorse"

        public string? ToOffice { get; set; }

        [MaxLength(500)]
        public string? Remarks { get; set; }
    }

    // ── Tracking (Module 3) ──

    public class TrackingResultViewModel
    {
        public bool Found { get; set; }
        public string TrackingNumber { get; set; } = string.Empty;
        public string? Title { get; set; }
        public string? DocumentType { get; set; }
        public string? SubmittedBy { get; set; }
        public string? Department { get; set; }
        public DocumentStatus? CurrentStatus { get; set; }
        public string? CurrentOffice { get; set; }
        public int CurrentStep { get; set; }
        public int TotalSteps { get; set; }
        public DateTime? DateFiled { get; set; }
        public DateTime? LastUpdated { get; set; }
        public List<TrackingTimelineItem> Timeline { get; set; } = new();
    }

    public class TrackingTimelineItem
    {
        public string Action { get; set; } = string.Empty;
        public string Office { get; set; } = string.Empty;
        public string? Remarks { get; set; }
        public DateTime Date { get; set; }
        public bool IsCurrent { get; set; }
    }

    // ── Oversight / Smart Escalation (Module 6) ──

    public class OversightDashboardViewModel
    {
        public int TotalActiveDocuments { get; set; }
        public int EscalatedCount { get; set; }
        public int WarningCount { get; set; }      // Documents nearing deadline (75%+)
        public int CriticalCount { get; set; }     // 7-day threshold breached
        public int OverdueCount { get; set; }      // 20-day threshold breached
        public int OnTrackCount { get; set; }
        public double OverallComplianceRate { get; set; }

        public List<EscalationAlertItem> EscalatedDocuments { get; set; } = new();
        public List<ARTAWarningItem> WarningDocuments { get; set; } = new();
        public Dictionary<string, int> OfficeViolations { get; set; } = new();

        public UserRole UserRole { get; set; }
    }

    public class ARTAWarningItem
    {
        public int DocumentId { get; set; }
        public string TrackingNumber { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string CurrentOffice { get; set; } = string.Empty;
        public int DaysElapsed { get; set; }
        public int ARTAPeriod { get; set; }
        public double PercentageUsed { get; set; }
    }

    // ── Admin ──

    public class AdminViewModel
    {
        public List<User> Users { get; set; } = new();
        public List<Department> Departments { get; set; } = new();
        public List<DocumentTypeConfig> DocumentTypes { get; set; } = new();
        public List<WorkflowStep> WorkflowSteps { get; set; } = new();
        public UserRole CurrentUserRole { get; set; }
    }

    public class UserCreateViewModel
    {
        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required]
        public string FullName { get; set; } = string.Empty;

        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? MobileNumber { get; set; }

        [Required]
        public UserRole Role { get; set; }

        public string Department { get; set; } = string.Empty;
        public int? DepartmentId { get; set; }
    }

    // ── Export (Module 7) ──

    public class ExportRequestViewModel
    {
        [Required(ErrorMessage = "Export password is required")]
        [DataType(DataType.Password)]
        public string ExportPassword { get; set; } = string.Empty;

        [Required]
        public string ExportType { get; set; } = "CSV"; // PDF, CSV, Excel

        public string? FilterDocType { get; set; }
        public string? FilterDepartment { get; set; }
        public string? FilterStatus { get; set; }
        public string? FilterDateFrom { get; set; }
        public string? FilterDateTo { get; set; }
    }

    // ── Reports (Module 7 — Oversight) ──

    public class ReportGenerateViewModel
    {
        [Required]
        public string ReportType { get; set; } = "Document Summary";

        [Required]
        public DateTime DateFrom { get; set; } = DateTime.UtcNow.AddMonths(-1);

        [Required]
        public DateTime DateTo { get; set; } = DateTime.UtcNow;

        // Report results
        public List<ReportDataRow> ReportData { get; set; } = new();
        public string? GeneratedReportHtml { get; set; }
        public bool IsGenerated { get; set; }
    }

    public class ReportDataRow
    {
        public string Label { get; set; } = string.Empty;
        public int Count { get; set; }
        public double? AverageDays { get; set; }
        public string? Extra { get; set; }
    }
}
