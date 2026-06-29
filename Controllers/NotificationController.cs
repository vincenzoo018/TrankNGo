using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TrackNGo.Services;

namespace TrackNGo.Controllers
{
    [Authorize]
    public class NotificationController : Controller
    {
        private readonly ISMSService _smsService;
        private readonly IAuthService _authService;

        public NotificationController(ISMSService smsService, IAuthService authService)
        {
            _smsService = smsService;
            _authService = authService;
        }

        // Screen 7 — SMS Notification Module (Module 5)
        public async Task<IActionResult> Index()
        {
            ViewData["UserFullName"] = User.FindFirst("FullName")?.Value ?? "User";
            var userRole = _authService.GetCurrentUserRole(User);
            ViewData["UserRole"] = userRole?.ToString() ?? "";

            var notifications = await _smsService.GetRecentAsync(100);
            
            // Role-based view routing
            var viewPath = userRole switch
            {
                TrackNGo.Models.UserRole.Mayor => "~/Views/MayorRole/Notification.cshtml",
                TrackNGo.Models.UserRole.ExecutiveAdmin => "~/Views/AdminRole/Notification.cshtml",
                TrackNGo.Models.UserRole.RecordsOfficer => "~/Views/RecordsRole/Notification.cshtml",
                TrackNGo.Models.UserRole.OversightOfficer => "~/Views/OversightRole/Notification.cshtml",
                _ => "~/Views/AdminRole/Notification.cshtml"
            };
            
            return View(viewPath, notifications);
        }
    }
}
