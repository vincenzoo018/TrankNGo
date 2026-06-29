namespace TrackNGo.Models
{
    /// <summary>
    /// Defines the four user roles in the TrackNGo system.
    /// NOTE: There is no "Client" role. Clients track documents via QR code only — no login.
    /// </summary>
    public enum UserRole
    {
        Mayor = 1,              // Local Chief Executive
        ExecutiveAdmin = 2,     // Executive Assistants & Administrators
        RecordsOfficer = 3,     // Records Officers & Receiving Clerks
        OversightOfficer = 4    // CART — Compliance, Accountability, Regulatory & Transparency Personnel
    }

    /// <summary>
    /// FSM-Based Document Lifecycle states.
    /// Represents the Finite State Machine states for document processing.
    /// </summary>
    public enum DocumentStatus
    {
        Submitted = 0,      // Just logged into the system
        Endorsed = 1,       // Classified and endorsed by Executive Admin
        UnderReview = 2,    // Being reviewed by assigned personnel
        ForApproval = 3,    // Forwarded to Mayor for approval
        Approved = 4,       // Mayor has approved and signed
        Returned = 5,       // Sent back for revision
        Rejected = 6,       // Permanently rejected
        ForRelease = 7,     // Approved and ready for release
        Completed = 8,      // Released to client/stakeholder
        Escalated = 9,      // Auto-escalated due to ARTA timeout
        Forwarded = 10,     // Forwarded to another department
        Accepted = 11       // Accepted by target department
    }

    /// <summary>
    /// SMS notification delivery status.
    /// </summary>
    public enum SMSStatus
    {
        Queued = 0,
        Sent = 1,
        Delivered = 2,
        Failed = 3
    }

    /// <summary>
    /// ARTA transaction complexity classification per RA 11032.
    /// Determines the processing period deadline.
    /// </summary>
    public enum ARTATransactionType
    {
        Simple = 3,          // 3 working days
        Complex = 7,         // 7 working days
        HighlyTechnical = 20 // 20 working days
    }
}
