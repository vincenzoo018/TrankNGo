using Microsoft.AspNetCore.Mvc;
using TrackNGo.Models;
using TrackNGo.Services;
using TrackNGo.ViewModels;

namespace TrackNGo.Controllers
{
    public class WorkflowController : BaseController
    {
        private readonly IWorkflowService _workflowService;
        private readonly IDocumentService _documentService;
        private readonly IAuditTrailService _audit;

        public WorkflowController(IWorkflowService workflowService, IDocumentService documentService,
            IAuditTrailService audit)
        {
            _workflowService = workflowService;
            _documentService = documentService;
            _audit = audit;
        }

        public async Task<IActionResult> Index()
        {
            var user = GetCurrentUser();
            // Mayor and ExecAdmin have access to the Workflow Routing dashboard
            if (user == null || (user.Role != UserRole.Mayor && user.Role != UserRole.ExecutiveAdmin))
                return RedirectToAction("Index", "Dashboard");

            var vm = await _documentService.GetDocumentListAsync(
                null, null, null, null, 1, 50); // Get first 50 active docs
            
            // Filter to only show active documents
            vm.Documents = vm.Documents
                .Where(d => d.Status != DocumentStatus.Completed && d.Status != DocumentStatus.Rejected)
                .ToList();

            // Role-based view routing
            var viewPath = user.Role == UserRole.Mayor 
                ? "~/Views/MayorRole/Workflow.cshtml" 
                : "~/Views/AdminRole/Workflow.cshtml";
            
            return View(viewPath, vm);
        }

        [HttpPost]
        public async Task<IActionResult> Transition(WorkflowActionViewModel model)
        {
            var user = GetCurrentUser();
            if (user == null) return Unauthorized();

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Invalid workflow action.";
                return RedirectToAction("ViewDocument", "Document", new { id = model.DocumentId });
            }

            DocumentStatus? nextStatus = model.Action switch
            {
                "Forward" => DocumentStatus.Forwarded,
                "Approve" => DocumentStatus.Approved,
                "Return" => DocumentStatus.Returned,
                "Reject" => DocumentStatus.Rejected,
                "Endorse" => DocumentStatus.Endorsed,
                "Review" => DocumentStatus.UnderReview,
                "Release" => DocumentStatus.ForRelease,
                "Complete" => DocumentStatus.Completed,
                "SubmitForApproval" => DocumentStatus.ForApproval,
                "Accept" => DocumentStatus.Accepted,
                _ => null
            };

            if (nextStatus == null)
            {
                TempData["ErrorMessage"] = "Unknown action.";
                return RedirectToAction("ViewDocument", "Document", new { id = model.DocumentId });
            }

            // Return action strictly requires remarks
            if (nextStatus == DocumentStatus.Returned && string.IsNullOrWhiteSpace(model.Remarks))
            {
                TempData["ErrorMessage"] = "Remarks are strictly required when returning a document.";
                return RedirectToAction("ViewDocument", "Document", new { id = model.DocumentId });
            }

            var success = await _workflowService.TransitionAsync(
                model.DocumentId,
                nextStatus.Value,
                user.Id,
                model.Action,
                model.ToOffice,
                model.Remarks);

            if (success)
            {
                TempData["SuccessMessage"] = $"Document successfully transitioned to {nextStatus}.";
            }
            else
            {
                TempData["ErrorMessage"] = "Workflow transition failed. You may not have permission or the transition is invalid for the current state.";
            }

            return RedirectToAction("ViewDocument", "Document", new { id = model.DocumentId });
        }

        [HttpPost]
        public async Task<IActionResult> AddComment(int documentId, string content, bool isInternal)
        {
            var user = GetCurrentUser();
            if (user == null) return Unauthorized();

            // RBAC: Only Mayor and ExecAdmin can add comments
            if (user.Role != UserRole.Mayor && user.Role != UserRole.ExecutiveAdmin)
            {
                TempData["ErrorMessage"] = "You do not have permission to add comments.";
                return RedirectToAction("ViewDocument", "Document", new { id = documentId });
            }

            if (string.IsNullOrWhiteSpace(content))
            {
                TempData["ErrorMessage"] = "Comment cannot be empty.";
                return RedirectToAction("ViewDocument", "Document", new { id = documentId });
            }

            await _workflowService.AddCommentAsync(documentId, user.Id, content, isInternal);
            TempData["SuccessMessage"] = "Comment added successfully.";

            return RedirectToAction("ViewDocument", "Document", new { id = documentId });
        }
    }
}
