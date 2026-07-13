using Microsoft.AspNetCore.Mvc;

namespace LibraryProject.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            
            if (!User.Identity?.IsAuthenticated ?? true)
                return RedirectToAction("Login", "Account");

            
            if (User.IsInRole("Admin"))
                return RedirectToAction("Index", "Home", new { area = "Admin" });
            if (User.IsInRole("Employee"))
                return RedirectToAction("Index", "Home", new { area = "Employee" });
            if (User.IsInRole("Kiosk"))
                return RedirectToAction("Index", "Kiosk");

            return RedirectToAction("Index", "Home", new { area = "Visitor" });
        }
    }
}