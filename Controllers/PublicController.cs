using Microsoft.AspNetCore.Mvc;
using TrackNGo.Services;
using TrackNGo.ViewModels;

namespace TrackNGo.Controllers
{
    /// <summary>
    /// Public-facing controller for external clients to track document status
    /// without requiring authentication. Accessed via QR code scanning.
    /// </summary>
    public class PublicController : Controller
    {
        private readonly IDocumentService _documentService;
        private readonly IQRCodeService _qrCodeService;

        public PublicController(IDocumentService documentService, IQRCodeService qrCodeService)
        {
            _documentService = documentService;
            _qrCodeService = qrCodeService;
        }

        // Public landing page
        public IActionResult Index()
        {
            return View();
        }

        // Public document tracking (accessed via QR code URL)
        // URL: /Public/Track?ref=TNG-2026-0312
        [HttpGet]
        public async Task<IActionResult> Track(string? refNumber)
        {
            if (string.IsNullOrWhiteSpace(refNumber))
                return View("Track", new TrackingResultViewModel { Found = false });

            var document = await _documentService.GetByTrackingNumberAsync(refNumber);

            // Increment scan count
            await _qrCodeService.IncrementScanCountAsync(refNumber);

            if (document == null)
            {
                return View("Track", new TrackingResultViewModel
                {
                    Found = false,
                    TrackingNumber = refNumber
                });
            }

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

            return View("Track", model);
        }
    }
}
