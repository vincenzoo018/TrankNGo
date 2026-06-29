using Microsoft.AspNetCore.Mvc;
using TrackNGo.Models;
using TrackNGo.Services;

namespace TrackNGo.Controllers
{
    public class SignatureController : BaseController
    {
        private readonly ISignatureService _signatureService;
        private readonly IDocumentService _documentService;
        private readonly IWorkflowService _workflowService;

        public SignatureController(ISignatureService signatureService, IDocumentService documentService,
            IWorkflowService workflowService)
        {
            _signatureService = signatureService;
            _documentService = documentService;
            _workflowService = workflowService;
        }

        public IActionResult Index()
        {
            var user = GetCurrentUser();
            // RBAC: Mayor and ExecAdmin can access digital signature
            if (user == null || (user.Role != UserRole.Mayor && user.Role != UserRole.ExecutiveAdmin))
                return RedirectToAction("Index", "Dashboard");

            // Role-based view routing
            var viewPath = user.Role == UserRole.Mayor 
                ? "~/Views/MayorRole/Signature.cshtml" 
                : "~/Views/AdminRole/Signature.cshtml";
            
            return View(viewPath);
        }

        [HttpPost]
        public async Task<IActionResult> SignDocument(int documentId, string signatureBase64, string actionType)
        {
            var user = GetCurrentUser();
            // Strict RBAC: Only Mayor can sign documents
            if (user == null || user.Role != UserRole.Mayor)
                return Unauthorized();

            var doc = await _documentService.GetByIdAsync(documentId);
            if (doc == null || doc.CurrentStatus != DocumentStatus.ForApproval)
                return BadRequest("Document is not pending approval.");

            // 1. Save digital signature
            await _signatureService.SignDocumentAsync(documentId, user.Id, signatureBase64, actionType);

            // 2. Transition state based on action (Approve vs Reject)
            var nextStatus = actionType == "Approved" ? DocumentStatus.Approved : DocumentStatus.Rejected;
            await _workflowService.TransitionAsync(documentId, nextStatus, user.Id, actionType,
                "Records Division", "Signed by Local Chief Executive");

            TempData["SuccessMessage"] = $"Document successfully signed and {actionType.ToLower()}.";
            return RedirectToAction("ViewDocument", "Document", new { id = documentId });
        }
    }
}
