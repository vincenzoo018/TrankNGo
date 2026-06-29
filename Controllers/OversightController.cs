using Microsoft.AspNetCore.Mvc;
using TrackNGo.Models;
using TrackNGo.Services;
using TrackNGo.ViewModels;

namespace TrackNGo.Controllers
{
    public class OversightController : BaseController
    {
        private readonly ISmartEscalationService _escalationService;
        private readonly IDocumentService _documentService;
        private readonly IReportService _reportService;

        public OversightController(ISmartEscalationService escalationService, IDocumentService documentService,
            IReportService reportService)
        {
            _escalationService = escalationService;
            _documentService = documentService;
            _reportService = reportService;
        }

        public async Task<IActionResult> Index()
        {
            var user = GetCurrentUser();
            // Both Mayor and Oversight can view the ARTA dashboard
            if (user == null || (user.Role != UserRole.OversightOfficer && user.Role != UserRole.Mayor && user.Role != UserRole.ExecutiveAdmin))
                return RedirectToAction("Index", "Dashboard");

            var escalated = await _escalationService.GetEscalatedDocumentsAsync();
            var warnings = await _escalationService.GetWarningDocumentsAsync();
            var complianceRate = await _escalationService.GetComplianceRateAsync();

            var vm = new OversightDashboardViewModel
            {
                TotalActiveDocuments = await _documentService.GetCountByStatusAsync(
                    DocumentStatus.Submitted, DocumentStatus.Endorsed, DocumentStatus.UnderReview,
                    DocumentStatus.ForApproval, DocumentStatus.Approved, DocumentStatus.ForRelease,
                    DocumentStatus.Forwarded, DocumentStatus.Accepted),
                EscalatedCount = escalated.Count,
                WarningCount = warnings.Count(w => w.PercentageUsed >= 75 && w.PercentageUsed < 100),
                CriticalCount = warnings.Count(w => w.DaysElapsed >= w.ARTAPeriod - 1 && w.DaysElapsed < w.ARTAPeriod),
                OverdueCount = escalated.Count(e => e.EscalationLevel == "Overdue"),
                OnTrackCount = await _documentService.GetTotalCountAsync() - escalated.Count - warnings.Count,
                OverallComplianceRate = complianceRate,
                EscalatedDocuments = escalated,
                WarningDocuments = warnings,
                UserRole = user.Role
            };

            // Role-based view routing
            var viewPath = user.Role switch
            {
                UserRole.OversightOfficer => "~/Views/OversightRole/ARTA.cshtml",
                UserRole.Mayor => "~/Views/MayorRole/ARTA.cshtml",
                UserRole.ExecutiveAdmin => "~/Views/AdminRole/ARTA.cshtml",
                _ => "~/Views/OversightRole/ARTA.cshtml"
            };
            
            return View(viewPath, vm);
        }

        [HttpPost]
        public async Task<IActionResult> Resolve(int documentId, string resolutionNotes)
        {
            var user = GetCurrentUser();
            // Strict RBAC: Only Oversight Officer can resolve escalations
            if (user == null || user.Role != UserRole.OversightOfficer)
                return Unauthorized("Only the Oversight Officer can resolve ARTA escalations.");

            if (string.IsNullOrWhiteSpace(resolutionNotes))
            {
                TempData["ErrorMessage"] = "Resolution notes are required.";
                return RedirectToAction("Index");
            }

            await _escalationService.ResolveEscalationAsync(documentId, user.Id, resolutionNotes);

            TempData["SuccessMessage"] = "Escalation resolved successfully.";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Reports()
        {
            var user = GetCurrentUser();
            if (user == null || user.Role != UserRole.OversightOfficer)
                return RedirectToAction("Index", "Dashboard");

            // Role-based view routing
            return View("~/Views/OversightRole/Reports.cshtml", new ReportGenerateViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> GenerateReport(ReportGenerateViewModel model)
        {
            var user = GetCurrentUser();
            if (user == null || user.Role != UserRole.OversightOfficer)
                return RedirectToAction("Index", "Dashboard");

            if (!ModelState.IsValid)
                return View("Reports", model);

            ReportGenerateViewModel result;
            switch (model.ReportType)
            {
                case "Department Performance":
                    result = await _reportService.GenerateDepartmentPerformanceAsync(model.DateFrom, model.DateTo, user.Id);
                    break;
                case "Workflow Report":
                    result = await _reportService.GenerateWorkflowReportAsync(model.DateFrom, model.DateTo, user.Id);
                    break;
                case "Document Summary":
                default:
                    result = await _reportService.GenerateDocumentSummaryAsync(model.DateFrom, model.DateTo, user.Id);
                    break;
            }

            return View("Reports", result);
        }

        [HttpGet]
        public IActionResult Export()
        {
            var user = GetCurrentUser();
            if (user == null || user.Role != UserRole.OversightOfficer)
                return RedirectToAction("Index", "Dashboard");

            return View(new ExportRequestViewModel());
        }
    }
}
