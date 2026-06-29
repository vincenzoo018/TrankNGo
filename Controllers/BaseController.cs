using Microsoft.AspNetCore.Mvc;
using TrackNGo.Models;

namespace TrackNGo.Controllers
{
    public class BaseController : Controller
    {
        // Simulated GetCurrentUser for now.
        // In a real application, this would read from HttpContext.User claims.
        protected User? GetCurrentUser()
        {
            var roleStr = HttpContext.Request.Query["role"].ToString();
            
            // Default to checking claims if available
            if (string.IsNullOrEmpty(roleStr))
            {
                var claimRole = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role)?.Value;
                if (!string.IsNullOrEmpty(claimRole) && Enum.TryParse<UserRole>(claimRole, out var parsedRole))
                {
                    var userId = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                    var userName = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Name)?.Value ?? "user";
                    var fullName = User.Claims.FirstOrDefault(c => c.Type == "FullName")?.Value ?? "User";
                    
                    if (int.TryParse(userId, out int id))
                    {
                        return new User { Id = id, Username = userName, FullName = fullName, Role = parsedRole };
                    }
                }
                roleStr = "admin"; // ultimate fallback
            }

            return roleStr.ToLower() switch
            {
                "mayor" => new User { Id = 2, Username = "mayor", FullName = "Hon. City Mayor", Role = UserRole.Mayor },
                "records" => new User { Id = 3, Username = "records", FullName = "Records Officer", Role = UserRole.RecordsOfficer },
                "cart" or "oversight" => new User { Id = 4, Username = "cart", FullName = "CART Security Officer", Role = UserRole.OversightOfficer },
                _ => new User { Id = 1, Username = "admin", FullName = "System Administrator", Role = UserRole.ExecutiveAdmin }
            };
        }
    }
}
