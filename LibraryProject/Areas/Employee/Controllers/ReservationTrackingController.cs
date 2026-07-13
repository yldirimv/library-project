using LibraryProject.Areas.Employee.Controllers;
using LibraryProject.Business.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LibraryProject.Web.Areas.Employee.Controllers
{
    public class ReservationTrackingController : EmployeeBaseController
    {
        private readonly IReservationService _reservationService;

        public ReservationTrackingController(IReservationService reservationService)
            => _reservationService = reservationService;

        public async Task<IActionResult> Index(int floor = 1)
        {
            ViewData["Floor"] = floor;
            ViewData["SeatStates"] = await _reservationService.GetCurrentSeatStatesAsync();
            ViewBag.TodaysReservations = await _reservationService.GetTodaysReservationsAsync();
            return View(await _reservationService.GetSeatsAsync(floor));
        }
    }
}