using LibraryProject.Areas.Visitor.Controllers;
using LibraryProject.Business.Interfaces;
using LibraryProject.Model.Enums;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LibraryProject.Web.Areas.Visitor.Controllers
{
    public class QrSimulationController : VisitorBaseController
    {
        private readonly ICheckInService _checkInService;

        public QrSimulationController(ICheckInService checkInService)
            => _checkInService = checkInService;

        public IActionResult Index() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Process(string token, ReservationEventType action)
        {
            var visitorId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var (ok, message) = await _checkInService.ProcessAsync(visitorId, token, action);

            TempData[ok ? "Success" : "Error"] = message;
            return RedirectToAction(nameof(Index));
        }
    }
}