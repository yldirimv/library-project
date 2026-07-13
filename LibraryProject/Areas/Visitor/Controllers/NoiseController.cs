using Azure.Core;
using LibraryProject.Areas.Visitor.Controllers;
using LibraryProject.Business.Interfaces;
using LibraryProject.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace LibraryProject.Web.Areas.Visitor.Controllers
{
    public class NoiseController : VisitorBaseController
    {
        private readonly INoiseReportService _noiseService;
        private readonly IHubContext<NotificationHub> _hub;

        public NoiseController(INoiseReportService noiseService,
                               IHubContext<NotificationHub> hub)
        {
            _noiseService = noiseService;
            _hub = hub;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Report()
        {
            var visitorId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var (ok, message, report) = await _noiseService.CreateAsync(visitorId);

            if (ok && report != null)
            {
                // Personel ekranlarına anlık it
                await _hub.Clients.Group("employees").SendAsync("NoiseReport",
                    new { seatNumber = report.Seat.SeatNumber, time = DateTime.Now.ToString("HH:mm") });
            }

            TempData[ok ? "Success" : "Error"] = message;
            return Redirect(Request.Headers["Referer"].ToString() ?? "/Visitor/Home");
        }
    }
}