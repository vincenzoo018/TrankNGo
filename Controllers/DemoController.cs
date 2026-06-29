using Microsoft.AspNetCore.Mvc;
using TrackNGo.Models;
using TrackNGo.Services;
using TrackNGo.ViewModels;

namespace TrackNGo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DemoController : ControllerBase
    {
        private readonly IDocumentService _documentService;
        private readonly IWorkflowService _workflowService;
        private readonly IAuthService _authService;

        public DemoController(IDocumentService documentService, IWorkflowService workflowService, IAuthService authService)
        {
            _documentService = documentService;
            _workflowService = workflowService;
            _authService = authService;
        }

        [HttpGet("run")]
        public async Task<IActionResult> RunDemo()
        {
            var logs = new List<string>();
            try
            {
                // Users (based on seed data)
                int recordsOfficerId = 3;
                int execAdminId = 1;
                int mayorId = 2;

                logs.Add("Starting Workflow Demo...");

                // 1. Records Officer Intakes Document
                var createModel = new DocumentCreateViewModel
                {
                    GeneratedTrackingNumber = await _documentService.GenerateTrackingNumberAsync(),
                    Title = "Demo Application " + DateTime.Now.Ticks,
                    DocumentType = "Application",
                    OriginatingDepartment = "Office of the Mayor",
                    SubmittedBy = "John Doe",
                    EmailAddress = "john@example.com",
                    ContactNumber = "09123456789"
                };
                
                var document = await _documentService.CreateDocumentAsync(createModel, recordsOfficerId);
                logs.Add($"[RecordsOfficer] Created Document: {document.TrackingNumber} (ID: {document.Id}). Status: {document.CurrentStatus}");

                // 2. ExecAdmin Endorses Document
                bool result = await _workflowService.TransitionAsync(document.Id, DocumentStatus.Endorsed, execAdminId, "Endorse", null, "Endorsed for review");
                logs.Add($"[ExecAdmin] Endorsed Document. Success: {result}");

                // 3. ExecAdmin forwards to Review
                result = await _workflowService.TransitionAsync(document.Id, DocumentStatus.UnderReview, execAdminId, "Review", "City Health Office", "Forwarding to CHO for review");
                logs.Add($"[ExecAdmin] Forwarded for Review. Success: {result}");

                // 4. ExecAdmin Submits for Approval
                result = await _workflowService.TransitionAsync(document.Id, DocumentStatus.ForApproval, execAdminId, "SubmitForApproval", "Mayor", "Reviewed, for approval");
                logs.Add($"[ExecAdmin] Submitted for Approval. Success: {result}");

                // 5. Mayor Approves
                result = await _workflowService.TransitionAsync(document.Id, DocumentStatus.Approved, mayorId, "Approve", null, "Approved by Mayor");
                logs.Add($"[Mayor] Approved Document. Success: {result}");

                // 6. ExecAdmin prepares for release
                result = await _workflowService.TransitionAsync(document.Id, DocumentStatus.ForRelease, execAdminId, "Release", null, "Ready for release");
                logs.Add($"[ExecAdmin] Prepared for Release. Success: {result}");

                // 7. RecordsOfficer completes
                result = await _workflowService.TransitionAsync(document.Id, DocumentStatus.Completed, recordsOfficerId, "Complete", null, "Released to client");
                logs.Add($"[RecordsOfficer] Completed Document. Success: {result}");

                // --- RETURN FLOW TEST ---
                logs.Add("--- Testing Return Flow ---");
                var createModel2 = new DocumentCreateViewModel
                {
                    GeneratedTrackingNumber = await _documentService.GenerateTrackingNumberAsync(),
                    Title = "Demo Return " + DateTime.Now.Ticks,
                    DocumentType = "Application",
                    OriginatingDepartment = "Office of the Mayor",
                    SubmittedBy = "John Doe",
                    EmailAddress = "john@example.com",
                    ContactNumber = "09123456789"
                };
                var doc2 = await _documentService.CreateDocumentAsync(createModel2, recordsOfficerId);
                logs.Add($"[RecordsOfficer] Created Document 2: {doc2.TrackingNumber} (ID: {doc2.Id}). Status: {doc2.CurrentStatus}");
                
                result = await _workflowService.TransitionAsync(doc2.Id, DocumentStatus.Endorsed, execAdminId, "Endorse", null, "Endorsed");
                result = await _workflowService.TransitionAsync(doc2.Id, DocumentStatus.UnderReview, execAdminId, "Review", null, "Reviewing");
                result = await _workflowService.TransitionAsync(doc2.Id, DocumentStatus.ForApproval, execAdminId, "SubmitForApproval", null, "For Approval");
                
                // Mayor Returns
                result = await _workflowService.TransitionAsync(doc2.Id, DocumentStatus.Returned, mayorId, "Return", null, "Missing attachments, return to clerk.");
                logs.Add($"[Mayor] Returned Document 2. Success: {result}");

                // RecordsOfficer resubmits
                result = await _workflowService.TransitionAsync(doc2.Id, DocumentStatus.Submitted, recordsOfficerId, "Resubmit", null, "Attachments added");
                logs.Add($"[RecordsOfficer] Resubmitted Document 2. Success: {result}");

                return Ok(logs);
            }
            catch (Exception ex)
            {
                logs.Add($"Error: {ex.Message}");
                logs.Add(ex.StackTrace);
                return StatusCode(500, logs);
            }
        }
    }
}
