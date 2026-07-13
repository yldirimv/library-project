using LibraryProject.Areas.Admin.Controllers;
using LibraryProject.Business.Interfaces;
using LibraryProject.Model.Entities;
using Microsoft.AspNetCore.Mvc;

namespace LibraryProject.Areas.Admin.Controllers
{
    public class AnnouncementController : AdminBaseController
    {
        private readonly IAnnouncementService _announcementService;

        public AnnouncementController(IAnnouncementService announcementService)
            => _announcementService = announcementService;

        public async Task<IActionResult> Index()
            => View(await _announcementService.GetAllAsync());

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]//csrf koruması
        public async Task<IActionResult> Create(Announcement announcement)
        {
            if (!ModelState.IsValid) return View(announcement);

            await _announcementService.CreateAsync(announcement);
            TempData["Success"] = "Duyuru eklendi.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var announcement = await _announcementService.GetByIdAsync(id);
            if (announcement == null) return NotFound();
            return View(announcement);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Announcement announcement)
        {
            if (!ModelState.IsValid) return View(announcement);

            await _announcementService.UpdateAsync(announcement);
            TempData["Success"] = "Duyuru güncellendi.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await _announcementService.DeleteAsync(id);
            TempData["Success"] = "Duyuru silindi.";
            return RedirectToAction(nameof(Index));
        }
    }
}