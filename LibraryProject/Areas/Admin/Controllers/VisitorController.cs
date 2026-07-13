using LibraryProject.Areas.Admin.Controllers;
using LibraryProject.Model.Entities;
using LibraryProject.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace LibraryProject.Areas.Admin.Controllers
{
    public class VisitorController : AdminBaseController
    {
        private readonly UserManager<AppUser> _userManager;

        public VisitorController(UserManager<AppUser> userManager)
            => _userManager = userManager;

        public async Task<IActionResult> Index(string? search)
        {
            var visitors = await _userManager.GetUsersInRoleAsync("Visitor");

            var filtered = string.IsNullOrWhiteSpace(search)
                ? visitors
                : visitors.Where(v => v.FullName.Contains(search,
                      StringComparison.OrdinalIgnoreCase)).ToList();

            ViewBag.Search = search;
            return View(filtered.OrderBy(v => v.FullName).ToList());
        }

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(VisitorCreateViewModel model)
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

            await _userManager.AddToRoleAsync(user, "Visitor");
            TempData["Success"] = "Ziyaretçi eklendi.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            return View(new VisitorEditViewModel
            {
                Id = user.Id,
                FullName = user.FullName,
                IdentityNumber = user.IdentityNumber ?? "",
                Email = user.Email!
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(VisitorEditViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null) return NotFound();

            user.FullName = model.FullName;
            user.IdentityNumber = model.IdentityNumber;
            user.Email = model.Email;
            user.UserName = model.Email;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error.Description);
                return View(model);
            }

            TempData["Success"] = "Ziyaretçi güncellendi.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            // Gerçek silme yerine hesabı süresiz kilitle (veri geçmişi korunur)
            await _userManager.SetLockoutEnabledAsync(user, true);
            await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);

            TempData["Success"] = $"{user.FullName} hesabı devre dışı bırakıldı.";
            return RedirectToAction(nameof(Index));
        }
    }
}