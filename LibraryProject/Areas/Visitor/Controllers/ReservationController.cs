using LibraryProject.Areas.Visitor.Controllers;
using LibraryProject.Business.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LibraryProject.Areas.Visitor.Controllers
{
    public class ReservationController : VisitorBaseController
    {
        private readonly IReservationService _reservationService;

        public ReservationController(IReservationService reservationService)
            => _reservationService = reservationService;

        private string CurrentUserId
            => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        // Sayfa: kroki + form
        public async Task<IActionResult> Index(int floor = 1)
        {
            ViewData["Floor"] = floor;
            return View(await _reservationService.GetSeatsAsync(floor));
        }

        // JSON: seçilen aralıkta dolu koltuk Id'leri
        [HttpGet]
        public async Task<IActionResult> Occupied(DateTime start, DateTime end)
            => Json(await _reservationService.GetOccupiedSeatIdsAsync(start, end));

        // Rezervasyonu oluştur
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int seatId, DateTime start, DateTime end)
        {
            var (ok, message) = await _reservationService.CreateAsync(
                CurrentUserId, seatId, start, end);

            TempData[ok ? "Success" : "Error"] = message;
            return RedirectToAction(ok ? "Index" : "Index", new { });
        }
    }
}