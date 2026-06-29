using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using TrackNGo.Services;
using TrackNGo.ViewModels;

namespace TrackNGo.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;
        private readonly IAuditTrailService _audit;

        public AuthController(IAuthService authService, IAuditTrailService audit)
        {
            _authService = authService;
            _audit = audit;
        }

        // Screen 1 — Login Page (GET)
        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Dashboard");

            return View();
        }

        // Login (POST)
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewData["Error"] = "Please fill in all fields.";
                ViewData["AttemptedUsername"] = model.Username;
                return View(model);
            }

            var user = await _authService.ValidateCredentialsAsync(model.Username, model.Password);
            if (user == null)
            {
                ViewData["Error"] = "Invalid credentials. The username or password you entered does not match any active account.";
                ViewData["AttemptedUsername"] = model.Username;
                return View(model);
            }

            var principal = await _authService.CreateClaimsPrincipalAsync(user);
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = model.RememberMe,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
                });

            await _audit.LogAsync(null, user.Id, "Login",
                $"User {user.Username} ({user.Role}) logged in successfully.",
                ipAddress: HttpContext.Connection.RemoteIpAddress?.ToString());

            return RedirectToAction("Index", "Dashboard");
        }

        // Logout
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            var userId = _authService.GetCurrentUserId(User);
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            if (userId.HasValue)
                await _audit.LogAsync(null, userId, "Logout", "User logged out.");

            return RedirectToAction("Login");
        }
    }
}
