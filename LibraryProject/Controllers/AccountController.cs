using LibraryProject.Model.Entities;
using LibraryProject.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace LibraryProject.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;

        public AccountController(UserManager<AppUser> userManager,//kullanıcı crud yapısı icin
                                 SignInManager<AppUser> signInManager)//oturum islemelerinin kapısı
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // ---------- LOGIN ----------
        public IActionResult Login() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError("", "E-posta veya şifre hatalı");
                return View(model);
            }

            if (await _userManager.IsLockedOutAsync(user))
            {
                ModelState.AddModelError("", "Bu hesap devre dışı bırakılmış.");
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(
                user, model.Password, model.RememberMe, lockoutOnFailure: false);

            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "E-posta veya şifre hatalı");
                return View(model);
            }

            // Role göre doğru panele yönlendir
            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Contains("Admin"))
                return RedirectToAction("Index", "Home", new { area = "Admin" });
            if (roles.Contains("Kiosk"))
                return RedirectToAction("Index", "Kiosk");
            if (roles.Contains("Employee"))
                return RedirectToAction("Index", "Home", new { area = "Employee" });

            return RedirectToAction("Index", "Home", new { area = "Visitor" });
        }

        // ---------- REGISTER ----------
        public IActionResult Register() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = new AppUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                IdentityNumber = model.IdentityNumber,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error.Description);
                return View(model);
            }

            // Webden kayıt olan herkes ziyaretçidir
            await _userManager.AddToRoleAsync(user, "Visitor");
            await _signInManager.SignInAsync(user, isPersistent: false);

            return RedirectToAction("Index", "Home", new { area = "Visitor" });
        }

        // ---------- LOGOUT ----------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }

        public IActionResult AccessDenied() => View();
    }
}