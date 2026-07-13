using LibraryProject.Areas.Visitor.Controllers;
using LibraryProject.Business.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LibraryProject.Web.Areas.Visitor.Controllers
{
    public class LoyaltyController : VisitorBaseController
    {
        private readonly ILoyaltyService _loyaltyService;

        public LoyaltyController(ILoyaltyService loyaltyService)
            => _loyaltyService = loyaltyService;

        private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        public async Task<IActionResult> Index()
        {
            ViewBag.Balance = await _loyaltyService.GetBalanceAsync(CurrentUserId);
            ViewBag.Gifts = await _loyaltyService.GetGiftsAsync();
            ViewBag.MyRequests = await _loyaltyService.GetMyRequestsAsync(CurrentUserId);
            return View(await _loyaltyService.GetTransactionsAsync(CurrentUserId));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Request(int giftId)
        {
            var (ok, message) = await _loyaltyService.RequestGiftAsync(CurrentUserId, giftId);
            TempData[ok ? "Success" : "Error"] = message;
            return RedirectToAction(nameof(Index));
        }
    }
}