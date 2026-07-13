using LibraryProject.Business.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LibraryProject.Api.Controllers
{
    [Route("api/reservations")]
    public class ReservationsController : ApiBaseController
    {
        private readonly IReservationService _reservationService;

        public ReservationsController(IReservationService reservationService)
            => _reservationService = reservationService;

        [HttpGet("mine")]
        public async Task<IActionResult> Mine()
        {
            var reservations = await _reservationService.GetMyReservationsAsync(CurrentUserId);

            // Entity'yi olduğu gibi dönme: döngüsel referans + gereksiz alanlar.
            // Mobilin ihtiyacı kadarını şekillendir:
            return Ok(reservations.Select(r => new
            {
                r.Id,
                seatNumber = r.Seat.SeatNumber,
                start = r.StartTime,
                end = r.EndTime,
                status = r.Status.ToString()
            }));
        }
    }
}