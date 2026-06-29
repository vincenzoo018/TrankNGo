using Microsoft.AspNetCore.Mvc;
using TrackNGo.Models;
using TrackNGo.Services;
using TrackNGo.ViewModels;
using System.Linq;

namespace TrackNGo.Controllers
{
    public class DashboardController : BaseController
    {
        private readonly IDocumentService _documentService;
        private readonly ISmartEscalationService _escalationService;
        private readonly IAuditTrailService _audit;

        public DashboardController(IDocumentService documentService, ISmartEscalationService escalationService, IAuditTrailService audit)
        {
            _documentService = documentService;
            _escalationService = escalationService;
            _audit = audit;
        }

        public async Task<IActionResult> Index()
        {
            var user = GetCurrentUser();
            if (user == null) return RedirectToAction("Login", "Auth");

            // Base view model with overall system stats
            var vm = new DashboardViewModel
            {
                TotalDocuments = await _documentService.GetTotalCountAsync(),
                InProgressCount = await _documentService.GetCountByStatusAsync(
                    DocumentStatus.Submitted, DocumentStatus.Endorsed, DocumentStatus.UnderReview,
                    DocumentStatus.ForApproval, DocumentStatus.ForRelease, DocumentStatus.Forwarded, DocumentStatus.Accepted),
                CompletedCount = await _documentService.GetCountByStatusAsync(DocumentStatus.Completed),
                PendingCount = await _documentService.GetCountByStatusAsync(DocumentStatus.ForApproval),
                EscalatedCount = await _documentService.GetCountByStatusAsync(DocumentStatus.Escalated),
                ReturnedCount = await _documentService.GetCountByStatusAsync(DocumentStatus.Returned),
                
                ARTAComplianceRate = await _escalationService.GetComplianceRateAsync(),
                StatusDistribution = await _documentService.GetStatusDistributionAsync(),
                DepartmentActivity = await _documentService.GetDepartmentActivityAsync(),
                
                UserFullName = user.FullName,
                UserRole = user.Role
            };

            // Role-specific metrics
            if (user.Role == UserRole.Mayor)
            {
                vm.PendingSignatureCount = await _documentService.GetCountByStatusAsync(DocumentStatus.ForApproval);
                vm.PendingApprovalCount = vm.PendingSignatureCount; // Mayor usually signs and approves simultaneously
                vm.ARTAFlaggedCount = await _documentService.GetCountByStatusAsync(DocumentStatus.Escalated);
            }
            else if (user.Role == UserRole.ExecutiveAdmin)
            {
                vm.NewlySubmittedCount = await _documentService.GetCountByStatusAsync(DocumentStatus.Submitted);
                vm.PendingMayorReviewCount = await _documentService.GetCountByStatusAsync(DocumentStatus.ForApproval);
            }
            else if (user.Role == UserRole.RecordsOfficer)
            {
                vm.ReceivedTodayCount = await _documentService.GetReceivedTodayCountAsync();
                vm.AwaitingResubmissionCount = await _documentService.GetCountByStatusAsync(DocumentStatus.Returned);
            }
            else if (user.Role == UserRole.OversightOfficer)
            {
                var escalated = await _escalationService.GetEscalatedDocumentsAsync();
                var warnings = await _escalationService.GetWarningDocumentsAsync();
                
                vm.WarningCount = warnings.Count(w => w.PercentageUsed >= 75 && w.PercentageUsed < 100);
                vm.CriticalCount = warnings.Count(w => w.DaysElapsed >= w.ARTAPeriod - 1 && w.DaysElapsed < w.ARTAPeriod);
                vm.OverdueCount = escalated.Count(e => e.EscalationLevel == "Overdue");
                
                vm.EscalationAlerts = escalated.Take(5).ToList();
            }

            // Populate recent activity for all roles
            var recentAudits = await _audit.GetRecentAsync(10);
            foreach (var a in recentAudits)
            {
                vm.RecentActivity.Add(new RecentActivityItem
                {
                    TrackingNumber = a.Document?.TrackingNumber ?? "System",
                    Action = a.Action,
                    PerformedBy = a.User?.FullName ?? "System",
                    Timestamp = a.Timestamp,
                    TimeAgo = GetTimeAgo(a.Timestamp)
                });
            }

            // Route to role-specific dashboard views
            var viewName = user.Role switch
            {
                UserRole.Mayor => "~/Views/MayorRole/Dashboard.cshtml",
                UserRole.RecordsOfficer => "~/Views/RecordsRole/Dashboard.cshtml",
                UserRole.OversightOfficer => "~/Views/OversightRole/Dashboard.cshtml",
                UserRole.ExecutiveAdmin => "~/Views/AdminRole/Dashboard.cshtml",
                _ => "Index" // Fallback
            };

            return View(viewName, vm);
        }

        private string GetTimeAgo(DateTime dt)
        {
            var span = DateTime.UtcNow - dt;
            if (span.Days > 0) return $"{span.Days}d ago";
            if (span.Hours > 0) return $"{span.Hours}h ago";
            if (span.Minutes > 0) return $"{span.Minutes}m ago";
            return "Just now";
        }
    }
}
