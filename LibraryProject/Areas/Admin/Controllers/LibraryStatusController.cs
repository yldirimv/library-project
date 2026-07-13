using LibraryProject.Areas.Admin.Controllers;
using LibraryProject.Business.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LibraryProject.Web.Areas.Admin.Controllers
{
    public class LibraryStatusController : AdminBaseController
    {
        private readonly IReservationService _reservationService;

        public LibraryStatusController(IReservationService reservationService)
            => _reservationService = reservationService;

        public async Task<IActionResult> Index(int floor = 1)
        {
            ViewData["Floor"] = floor;
            ViewData["SeatStates"] = await _reservationService.GetCurrentSeatStatesAsync();
            return View(await _reservationService.GetSeatsAsync(floor));
        }
    }
}