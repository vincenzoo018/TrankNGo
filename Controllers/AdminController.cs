using Microsoft.AspNetCore.Mvc;
using TrackNGo.Data;
using TrackNGo.Models;
using TrackNGo.Services;
using TrackNGo.ViewModels;

namespace TrackNGo.Controllers
{
    public class AdminController : BaseController
    {
        private readonly ApplicationDbContext _db;
        private readonly IAuditTrailService _audit;
        private readonly ILogger<AdminController> _logger;

        public AdminController(ApplicationDbContext db, IAuditTrailService audit, ILogger<AdminController> logger)
        {
            _db = db;
            _audit = audit;
            _logger = logger;
        }

        public IActionResult Index()
        {
            var user = GetCurrentUser();
            if (user == null || (user.Role != UserRole.ExecutiveAdmin && user.Role != UserRole.OversightOfficer))
                return RedirectToAction("Index", "Dashboard");

            var users = _db.Users.ToList();
            var departments = _db.Departments.ToList();
            var documentTypes = _db.DocumentTypeConfigs.ToList();
            var workflowSteps = _db.WorkflowSteps.ToList();
            
            var vm = new AdminViewModel
            {
                Users = users,
                Departments = departments,
                DocumentTypes = documentTypes,
                WorkflowSteps = workflowSteps,
                CurrentUserRole = user.Role
            };
            
            // Role-based view routing
            var viewPath = user.Role == UserRole.ExecutiveAdmin 
                ? "~/Views/AdminRole/UserManagement.cshtml" 
                : "~/Views/OversightRole/UserManagement.cshtml";
            
            return View(viewPath, vm);
        }

        [HttpGet]
        public IActionResult CreateUser()
        {
            var user = GetCurrentUser();
            if (user == null || (user.Role != UserRole.ExecutiveAdmin && user.Role != UserRole.OversightOfficer))
                return RedirectToAction("Index", "Dashboard");

            return View(new UserCreateViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser(UserCreateViewModel model)
        {
            var adminUser = GetCurrentUser();
            if (adminUser == null || (adminUser.Role != UserRole.ExecutiveAdmin && adminUser.Role != UserRole.OversightOfficer))
                return RedirectToAction("Index", "Dashboard");

            if (!ModelState.IsValid)
                return View(model);

            // Simple hash for demo (in production use ASP.NET Identity or proper PBKDF2)
            var hash = Convert.ToBase64String(System.Security.Cryptography.SHA256.Create()
                .ComputeHash(System.Text.Encoding.UTF8.GetBytes(model.Password)));

            var newUser = new User
            {
                Username = model.Username,
                PasswordHash = hash,
                FullName = model.FullName,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                MobileNumber = model.MobileNumber,
                Role = model.Role,
                Department = model.Department,
                DepartmentId = model.DepartmentId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _db.Users.Add(newUser);
            await _db.SaveChangesAsync();

            await _audit.LogAsync(null, adminUser.Id, "UserCreated",
                $"Created new user: {model.Username} ({model.Role})");

            TempData["SuccessMessage"] = "User created successfully.";
            return RedirectToAction("Index");
        }
        
        [HttpPost]
        public async Task<IActionResult> ResetPassword(int userId, string newPassword)
        {
             var adminUser = GetCurrentUser();
            if (adminUser == null || (adminUser.Role != UserRole.ExecutiveAdmin && adminUser.Role != UserRole.OversightOfficer))
                return RedirectToAction("Index", "Dashboard");
                
            var user = await _db.Users.FindAsync(userId);
            if (user != null && !string.IsNullOrEmpty(newPassword))
            {
                 var hash = Convert.ToBase64String(System.Security.Cryptography.SHA256.Create()
                    .ComputeHash(System.Text.Encoding.UTF8.GetBytes(newPassword)));
                user.PasswordHash = hash;
                await _db.SaveChangesAsync();
                
                 await _audit.LogAsync(null, adminUser.Id, "PasswordReset",
                    $"Reset password for user: {user.Username}");
                    
                TempData["SuccessMessage"] = "Password reset successfully.";
            }
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Audit(int page = 1)
        {
            var user = GetCurrentUser();
            // Both Admin and Oversight can view audit logs
            if (user == null || (user.Role != UserRole.ExecutiveAdmin && user.Role != UserRole.OversightOfficer))
                return RedirectToAction("Index", "Dashboard");

            int pageSize = 50;
            var logs = await _audit.GetAllAsync(page, pageSize);
            
            // Role-based view routing
            var viewPath = user.Role == UserRole.ExecutiveAdmin 
                ? "~/Views/AdminRole/AuditTrail.cshtml" 
                : "~/Views/OversightRole/AuditTrail.cshtml";
            
            return View(viewPath, logs);
        }
    }
}
