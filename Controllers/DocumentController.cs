using Microsoft.AspNetCore.Mvc;
using TrackNGo.Models;
using TrackNGo.Services;
using TrackNGo.ViewModels;
using System.IO;

namespace TrackNGo.Controllers
{
    public class DocumentController : BaseController
    {
        private readonly IDocumentService _documentService;
        private readonly IQRCodeService _qrService;
        private readonly IAuditTrailService _audit;
        private readonly ISMSService _sms;
        private readonly IWebHostEnvironment _env;

        public DocumentController(IDocumentService documentService, IQRCodeService qrService,
            IAuditTrailService audit, ISMSService sms, IWebHostEnvironment env)
        {
            _documentService = documentService;
            _qrService = qrService;
            _audit = audit;
            _sms = sms;
            _env = env;
        }

        public async Task<IActionResult> Index(string? search, string? docType, string? department, string? status, int page = 1)
        {
            var user = GetCurrentUser();
            if (user == null) return RedirectToAction("Login", "Auth");

            var vm = await _documentService.GetDocumentListAsync(search, docType, department, status, page);
            vm.UserRole = user.Role;
            
            // Role-based view routing
            var viewPath = user.Role switch
            {
                UserRole.Mayor => "~/Views/MayorRole/Document.cshtml",
                UserRole.ExecutiveAdmin => "~/Views/AdminRole/Document.cshtml",
                UserRole.RecordsOfficer => "~/Views/RecordsRole/Document.cshtml",
                UserRole.OversightOfficer => "~/Views/OversightRole/Document.cshtml",
                _ => "~/Views/AdminRole/Document.cshtml"
            };
            
            return View(viewPath, vm);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var user = GetCurrentUser();
            if (user == null) return RedirectToAction("Login", "Auth");

            // RBAC: Only RecordsOfficer and ExecAdmin can intake documents
            if (user.Role != UserRole.RecordsOfficer && user.Role != UserRole.ExecutiveAdmin)
            {
                TempData["ErrorMessage"] = "You do not have permission to intake new documents.";
                return RedirectToAction("Index");
            }

            var vm = new DocumentCreateViewModel
            {
                GeneratedTrackingNumber = await _documentService.GenerateTrackingNumberAsync(),
                DocumentTypes = await _documentService.GetDocumentTypesAsync(),
                DepartmentOptions = await _documentService.GetDepartmentOptionsAsync()
            };
            
            // Role-based view routing (only Admin and Records can reach here)
            var viewPath = user.Role == UserRole.RecordsOfficer 
                ? "~/Views/RecordsRole/DocumentIntake.cshtml" 
                : "~/Views/AdminRole/Document.cshtml";
            
            return View(viewPath, vm);
        }

        [HttpPost]
        public async Task<IActionResult> Create(DocumentCreateViewModel model, List<IFormFile> attachments)
        {
            var user = GetCurrentUser();
            if (user == null) return RedirectToAction("Login", "Auth");

            if (user.Role != UserRole.RecordsOfficer && user.Role != UserRole.ExecutiveAdmin)
                return Unauthorized();

            if (!ModelState.IsValid)
            {
                model.DocumentTypes = await _documentService.GetDocumentTypesAsync();
                model.DepartmentOptions = await _documentService.GetDepartmentOptionsAsync();
                return View(model);
            }

            // Create document, metadata, and routing slip
            var document = await _documentService.CreateDocumentAsync(model, user.Id);

            // Handle multiple file uploads (Table 6 alignment)
            if (attachments != null && attachments.Count > 0)
            {
                var uploadDir = Path.Combine(_env.WebRootPath, "uploads", document.TrackingNumber);
                Directory.CreateDirectory(uploadDir);

                foreach (var file in attachments)
                {
                    if (file.Length > 0)
                    {
                        var fileName = Path.GetFileName(file.FileName);
                        var filePath = Path.Combine(uploadDir, fileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        // We can't access DB context directly since it's injected in service,
                        // but let's assume we update the document directly here for demo
                        var doc = await _documentService.GetByIdAsync(document.Id);
                        if (doc != null)
                        {
                            // In real scenario, IDocumentService should handle this
                            // Fallback to updating AttachmentPath for legacy views
                            if (string.IsNullOrEmpty(doc.AttachmentPath))
                            {
                                doc.AttachmentPath = $"/uploads/{document.TrackingNumber}/{fileName}";
                                await _documentService.UpdateDocumentAsync(doc);
                            }
                        }
                    }
                }
            }

            // Generate QR Code
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            await _qrService.GenerateQRCodeAsync(document, baseUrl);

            // Log Audit Trail
            await _audit.LogAsync(document.Id, user.Id, "DocumentCreated",
                $"Document {document.TrackingNumber} received and registered by {user.FullName}");

            // Send SMS Notification
            await _sms.SendTemplatedNotificationAsync(document, "DocumentReceived");

            TempData["SuccessMessage"] = $"Document {document.TrackingNumber} successfully registered.";
            return RedirectToAction("ViewDocument", new { id = document.Id });
        }

        public async Task<IActionResult> ViewDocument(int id)
        {
            var user = GetCurrentUser();
            if (user == null) return RedirectToAction("Login", "Auth");

            var vm = await _documentService.GetDocumentDetailAsync(id, user.Role);
            if (vm.Id == 0) return NotFound();

            return View(vm);
        }
    }
}
