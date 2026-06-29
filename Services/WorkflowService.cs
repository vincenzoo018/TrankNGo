using Microsoft.EntityFrameworkCore;
using TrackNGo.Data;
using TrackNGo.Models;

namespace TrackNGo.Services
{
    public interface IWorkflowService
    {
        /// <summary>Validate whether a transition from one status to another is allowed per FSM rules.</summary>
        bool IsValidTransition(DocumentStatus from, DocumentStatus to, UserRole role);

        /// <summary>Execute a workflow transition (state change) on a document.</summary>
        Task<bool> TransitionAsync(int documentId, DocumentStatus toStatus, int userId,
            string actionTaken, string? toOffice = null, string? remarks = null);

        /// <summary>Get all valid next states for a document given the user's role.</summary>
        List<DocumentStatus> GetValidNextStates(DocumentStatus currentStatus, UserRole role);

        /// <summary>Add a remark/comment to a document.</summary>
        Task AddCommentAsync(int documentId, int userId, string content, bool isInternal = true, string remarkType = "Comment");
    }

    public class WorkflowService : IWorkflowService
    {
        private readonly ApplicationDbContext _db;
        private readonly IAuditTrailService _audit;

        public WorkflowService(ApplicationDbContext db, IAuditTrailService audit)
        {
            _db = db;
            _audit = audit;
        }

        /// <summary>
        /// FSM Transition Rules Matrix.
        /// Maps (FromStatus, ToStatus) → allowed UserRoles.
        /// Aligned to the 4-role workflow specification.
        /// </summary>
        private static readonly Dictionary<(DocumentStatus, DocumentStatus), UserRole[]> TransitionRules = new()
        {
            // ── Records Officer submits → Endorsed by ExecAdmin ──
            { (DocumentStatus.Submitted, DocumentStatus.Endorsed), new[] { UserRole.ExecutiveAdmin } },

            // ── Submitted → UnderReview (RecordsOfficer forwards) ──
            { (DocumentStatus.Submitted, DocumentStatus.UnderReview), new[] { UserRole.RecordsOfficer, UserRole.ExecutiveAdmin } },

            // ── Endorsed → UnderReview (ExecAdmin routes to department) ──
            { (DocumentStatus.Endorsed, DocumentStatus.UnderReview), new[] { UserRole.ExecutiveAdmin } },

            // ── Endorsed → Returned (ExecAdmin returns for incomplete docs) ──
            { (DocumentStatus.Endorsed, DocumentStatus.Returned), new[] { UserRole.ExecutiveAdmin } },

            // ── UnderReview → ForApproval (ExecAdmin completes review) ──
            { (DocumentStatus.UnderReview, DocumentStatus.ForApproval), new[] { UserRole.ExecutiveAdmin } },

            // ── UnderReview → Returned (ExecAdmin returns for revision) ──
            { (DocumentStatus.UnderReview, DocumentStatus.Returned), new[] { UserRole.ExecutiveAdmin } },

            // ── Returned → Submitted (Clerk resubmits corrected document) ──
            { (DocumentStatus.Returned, DocumentStatus.Submitted), new[] { UserRole.RecordsOfficer } },

            // ── Returned → UnderReview (Resubmitted for review) ──
            { (DocumentStatus.Returned, DocumentStatus.UnderReview), new[] { UserRole.RecordsOfficer, UserRole.ExecutiveAdmin } },

            // ── ForApproval → Approved (Mayor approves and signs) ──
            { (DocumentStatus.ForApproval, DocumentStatus.Approved), new[] { UserRole.Mayor } },

            // ── ForApproval → Returned (Mayor returns with comments) ──
            { (DocumentStatus.ForApproval, DocumentStatus.Returned), new[] { UserRole.Mayor } },

            // ── ForApproval → Rejected (Mayor rejects) ──
            { (DocumentStatus.ForApproval, DocumentStatus.Rejected), new[] { UserRole.Mayor } },

            // ── Approved → ForRelease (ExecAdmin prepares for release) ──
            { (DocumentStatus.Approved, DocumentStatus.ForRelease), new[] { UserRole.ExecutiveAdmin } },

            // ── ForRelease → Completed (RecordsOfficer releases to client) ──
            { (DocumentStatus.ForRelease, DocumentStatus.Completed), new[] { UserRole.RecordsOfficer, UserRole.ExecutiveAdmin } },

            // ── Escalated → ForApproval (Mayor reviews escalated document) ──
            { (DocumentStatus.Escalated, DocumentStatus.ForApproval), new[] { UserRole.Mayor, UserRole.ExecutiveAdmin } },

            // ── Any active → Forwarded (Forward to another department) ──
            { (DocumentStatus.Submitted, DocumentStatus.Forwarded), new[] { UserRole.RecordsOfficer, UserRole.ExecutiveAdmin } },
            { (DocumentStatus.UnderReview, DocumentStatus.Forwarded), new[] { UserRole.ExecutiveAdmin } },
            { (DocumentStatus.Endorsed, DocumentStatus.Forwarded), new[] { UserRole.ExecutiveAdmin } },
            { (DocumentStatus.Forwarded, DocumentStatus.UnderReview), new[] { UserRole.RecordsOfficer, UserRole.ExecutiveAdmin } },
            { (DocumentStatus.Forwarded, DocumentStatus.Accepted), new[] { UserRole.RecordsOfficer, UserRole.ExecutiveAdmin } },
            { (DocumentStatus.Accepted, DocumentStatus.UnderReview), new[] { UserRole.ExecutiveAdmin } },
        };

        public bool IsValidTransition(DocumentStatus from, DocumentStatus to, UserRole role)
        {
            var key = (from, to);
            if (!TransitionRules.ContainsKey(key)) return false;
            return TransitionRules[key].Contains(role);
        }

        public List<DocumentStatus> GetValidNextStates(DocumentStatus currentStatus, UserRole role)
        {
            var validStates = new List<DocumentStatus>();
            foreach (var rule in TransitionRules)
            {
                if (rule.Key.Item1 == currentStatus && rule.Value.Contains(role))
                    validStates.Add(rule.Key.Item2);
            }
            return validStates;
        }

        public async Task<bool> TransitionAsync(int documentId, DocumentStatus toStatus, int userId,
            string actionTaken, string? toOffice = null, string? remarks = null)
        {
            var document = await _db.Documents.FindAsync(documentId);
            if (document == null) return false;

            // Check if document is locked (escalated)
            if (document.IsLocked && toStatus != DocumentStatus.ForApproval)
                return false;

            var user = await _db.Users.FindAsync(userId);
            if (user == null) return false;

            // Validate FSM transition
            if (!IsValidTransition(document.CurrentStatus, toStatus, user.Role))
                return false;

            // For Return action, remarks are required
            if (toStatus == DocumentStatus.Returned && string.IsNullOrWhiteSpace(remarks))
                return false;

            var fromStatus = document.CurrentStatus;
            var fromOffice = document.CurrentOfficeName;

            // Record the transition
            var transition = new WorkflowTransition
            {
                DocumentId = documentId,
                FromStatus = fromStatus,
                ToStatus = toStatus,
                FromOffice = fromOffice,
                ToOffice = toOffice ?? document.CurrentOfficeName,
                ActionTaken = actionTaken,
                Remarks = remarks,
                PerformedByUserId = userId,
                TransitionDate = DateTime.UtcNow,
                StepNumber = document.CurrentStepIndex + 1
            };
            _db.WorkflowTransitions.Add(transition);

            // Update document state
            document.CurrentStatus = toStatus;
            if (!string.IsNullOrEmpty(toOffice))
                document.CurrentOfficeName = toOffice;
            document.CurrentStepIndex = transition.StepNumber;
            document.LastUpdated = DateTime.UtcNow;

            // Handle completion
            if (toStatus == DocumentStatus.Completed)
            {
                document.DateCompleted = DateTime.UtcNow;

                // Update routing slip status
                var slip = await _db.RoutingSlips.FirstOrDefaultAsync(r => r.DocumentId == documentId);
                if (slip != null)
                    slip.SlipStatus = "Completed";
            }

            // Handle return — update routing slip
            if (toStatus == DocumentStatus.Returned)
            {
                var slip = await _db.RoutingSlips.FirstOrDefaultAsync(r => r.DocumentId == documentId);
                if (slip != null)
                    slip.SlipStatus = "Returned";

                // Add Return Remark automatically
                if (!string.IsNullOrWhiteSpace(remarks))
                {
                    var returnRemark = new DocumentComment
                    {
                        DocumentId = documentId,
                        UserId = userId,
                        Content = remarks,
                        PostedAt = DateTime.UtcNow,
                        IsInternal = false,
                        RemarkType = "Return Remark"
                    };
                    _db.DocumentComments.Add(returnRemark);
                }
            }

            // Handle resubmission from Returned state
            if (fromStatus == DocumentStatus.Returned && (toStatus == DocumentStatus.Submitted || toStatus == DocumentStatus.UnderReview))
            {
                var slip = await _db.RoutingSlips.FirstOrDefaultAsync(r => r.DocumentId == documentId);
                if (slip != null)
                    slip.SlipStatus = "Active";
            }

            // Unlock if escalated document is being reviewed
            if (fromStatus == DocumentStatus.Escalated && toStatus == DocumentStatus.ForApproval)
            {
                document.IsLocked = false;
            }

            // Handle endorsement by Mayor — update routing slip noted_by
            if (toStatus == DocumentStatus.Endorsed || (actionTaken == "Endorse" && user.Role == UserRole.ExecutiveAdmin))
            {
                var slip = await _db.RoutingSlips.FirstOrDefaultAsync(r => r.DocumentId == documentId);
                if (slip != null && slip.NotedByUserId == null)
                {
                    // Exec admin notes the slip during endorsement
                }
            }

            await _db.SaveChangesAsync();

            // Log audit trail
            await _audit.LogAsync(documentId, userId,
                "StatusChanged",
                $"Document {document.TrackingNumber} transitioned from {fromStatus} to {toStatus} by {user.FullName}. Action: {actionTaken}",
                fromStatus.ToString(), toStatus.ToString());

            return true;
        }

        public async Task AddCommentAsync(int documentId, int userId, string content, bool isInternal = true, string remarkType = "Comment")
        {
            var comment = new DocumentComment
            {
                DocumentId = documentId,
                UserId = userId,
                Content = content,
                PostedAt = DateTime.UtcNow,
                IsInternal = isInternal,
                RemarkType = remarkType
            };
            _db.DocumentComments.Add(comment);
            await _db.SaveChangesAsync();

            await _audit.LogAsync(documentId, userId, "CommentAdded",
                $"Remark added to document (Type: {remarkType}, Internal: {isInternal})");
        }
    }
}
