using LibraryProject.Areas.Admin.Controllers;
using LibraryProject.Business.Interfaces;
using LibraryProject.Model.Entities;
using Microsoft.AspNetCore.Mvc;

namespace LibraryProject.Web.Areas.Admin.Controllers
{
    public class LoyaltyController : AdminBaseController
    {
        private readonly ILoyaltyService _loyaltyService;

        public LoyaltyController(ILoyaltyService loyaltyService)
            => _loyaltyService = loyaltyService;

        //talep ve hediyeler tek sayfad
        public async Task<IActionResult> Index()
        {
            ViewBag.PendingRequests = await _loyaltyService.GetPendingRequestsAsync();
            return View(await _loyaltyService.GetGiftsAsync());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var (ok, message) = await _loyaltyService.ApproveRequestAsync(id);
            TempData[ok ? "Success" : "Error"] = message;
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id)
        {
            await _loyaltyService.RejectRequestAsync(id);
            TempData["Success"] = "Talep reddedildi.";
            return RedirectToAction(nameof(Index));
        }

        public IActionResult CreateGift() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateGift(Gift gift)
        {
            if (!ModelState.IsValid) return View(gift);
            await _loyaltyService.CreateGiftAsync(gift);
            TempData["Success"] = "Hediye eklendi.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> EditGift(int id)
        {
            var gift = await _loyaltyService.GetGiftByIdAsync(id);
            if (gift == null) return NotFound();
            return View(gift);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditGift(Gift gift)
        {
            if (!ModelState.IsValid) return View(gift);
            await _loyaltyService.UpdateGiftAsync(gift);
            TempData["Success"] = "Hediye güncellendi.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteGift(int id)
        {
            await _loyaltyService.DeleteGiftAsync(id);
            TempData["Success"] = "Hediye silindi.";
            return RedirectToAction(nameof(Index));
        }
    }
}