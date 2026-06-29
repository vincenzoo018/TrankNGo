using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TrackNGo.Services;
using TrackNGo.ViewModels;

namespace TrackNGo.Controllers
{
    [Authorize]
    public class TrackingController : Controller
    {
        private readonly IDocumentService _documentService;
        private readonly IQRCodeService _qrCodeService;
        private readonly IAuthService _authService;

        public TrackingController(
            IDocumentService documentService,
            IQRCodeService qrCodeService,
            IAuthService authService)
        {
            _documentService = documentService;
            _qrCodeService = qrCodeService;
            _authService = authService;
        }

        // Screen 5 — QR Code Tracking Page (Module 3)
        public IActionResult Index()
        {
            ViewData["UserFullName"] = User.FindFirst("FullName")?.Value ?? "User";
            var userRole = _authService.GetCurrentUserRole(User);
            ViewData["UserRole"] = userRole?.ToString() ?? "";
            
            // Role-based view routing
            var viewPath = userRole switch
            {
                TrackNGo.Models.UserRole.Mayor => "~/Views/MayorRole/Tracking.cshtml",
                TrackNGo.Models.UserRole.ExecutiveAdmin => "~/Views/AdminRole/Tracking.cshtml",
                TrackNGo.Models.UserRole.RecordsOfficer => "~/Views/RecordsRole/Tracking.cshtml",
                TrackNGo.Models.UserRole.OversightOfficer => "~/Views/OversightRole/Tracking.cshtml",
                _ => "~/Views/AdminRole/Tracking.cshtml"
            };
            
            return View(viewPath);
        }

        // Search by tracking number
        [HttpGet]
        public async Task<IActionResult> Search(string trackingNumber)
        {
            if (string.IsNullOrWhiteSpace(trackingNumber))
                return RedirectToAction("Index");

            var document = await _documentService.GetByTrackingNumberAsync(trackingNumber);
            if (document == null)
            {
                TempData["Error"] = $"No document found with tracking number: {trackingNumber}";
                return RedirectToAction("Index");
            }

            // Increment scan count
            await _qrCodeService.IncrementScanCountAsync(trackingNumber);

            var model = new TrackingResultViewModel
            {
                Found = true,
                TrackingNumber = document.TrackingNumber,
                Title = document.Title,
                DocumentType = document.DocumentType,
                SubmittedBy = document.SubmittedBy,
                Department = document.OriginatingDepartment,
                CurrentStatus = document.CurrentStatus,
                CurrentOffice = document.CurrentOfficeName,
                CurrentStep = document.CurrentStepIndex,
                TotalSteps = document.TotalSteps,
                DateFiled = document.DateFiled,
                LastUpdated = document.LastUpdated,
                Timeline = document.Transitions
                    .OrderBy(t => t.TransitionDate)
                    .Select(t => new TrackingTimelineItem
                    {
                        Action = t.ActionTaken,
                        Office = t.ToOffice,
                        Remarks = t.Remarks,
                        Date = t.TransitionDate,
                        IsCurrent = false
                    }).ToList()
            };

            if (model.Timeline.Any())
                model.Timeline.Last().IsCurrent = true;

            return View("Result", model);
        }

        public IActionResult Feedback()
        {
            ViewData["UserFullName"] = User.FindFirst("FullName")?.Value ?? "User";
            ViewData["UserRole"] = _authService.GetCurrentUserRole(User)?.ToString() ?? "";
            return View();
        }
    }
}
