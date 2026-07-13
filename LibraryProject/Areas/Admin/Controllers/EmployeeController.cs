using LibraryProject.Areas.Admin.Controllers;
using LibraryProject.Model.Entities;
using LibraryProject.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace LibraryProject.Areas.Admin.Controllers
{
    //arada servis katmanı yok. user manager halihazırda identitynin hazır servis katmanı
    //yani kendi entitylerimde repo/uow, identity varlıklarında onun kendi yoneticileri isler
    public class EmployeeController : AdminBaseController
    {
        private readonly UserManager<AppUser> _userManager;

        public EmployeeController(UserManager<AppUser> userManager)
            => _userManager = userManager;

        public async Task<IActionResult> Index()
        {
            // Employee rolündeki tüm kullanıcılar
            var employees = await _userManager.GetUsersInRoleAsync("Employee");
            return View(employees.OrderBy(e => e.FullName).ToList());
        }

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EmployeeCreateViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = new AppUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error.Description);
                return View(model);
            }

            await _userManager.AddToRoleAsync(user, "Employee");
            TempData["Success"] = "Çalışan eklendi.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            return View(new EmployeeEditViewModel
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email!
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EmployeeEditViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null) return NotFound();

            user.FullName = model.FullName;
            user.Email = model.Email;
            user.UserName = model.Email;   // login e-postayla, ikisi senkron 

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error.Description);
                return View(model);
            }

            TempData["Success"] = "Çalışan güncellendi.";
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