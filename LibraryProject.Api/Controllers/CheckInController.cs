using LibraryProject.Business.Interfaces;
using LibraryProject.Model.Enums;
using Microsoft.AspNetCore.Mvc;

namespace LibraryProject.Api.Controllers
{
    [Route("api/checkin")]
    public class CheckInController : ApiBaseController
    {
        private readonly ICheckInService _checkInService;

        public CheckInController(ICheckInService checkInService)
            => _checkInService = checkInService;

        public record CheckInRequest(string Token, string Action);

        [HttpPost]
        public async Task<IActionResult> Process(CheckInRequest request)
        {
            if (!Enum.TryParse<ReservationEventType>(request.Action, out var action))
                return BadRequest(new { message = "Geçersiz işlem tipi." });

            var (ok, message) = await _checkInService.ProcessAsync(
                CurrentUserId, request.Token, action);

            return ok ? Ok(new { message }) : BadRequest(new { message });
        }
    }
}