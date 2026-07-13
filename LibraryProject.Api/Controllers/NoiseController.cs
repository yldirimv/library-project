using LibraryProject.Business.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LibraryProject.Api.Controllers
{
    [Route("api/noise")]
    public class NoiseController : ApiBaseController
    {
        private readonly INoiseReportService _noiseService;

        public NoiseController(INoiseReportService noiseService)
            => _noiseService = noiseService;

        [HttpPost]
        public async Task<IActionResult> Report()
        {
            var (ok, message, _) = await _noiseService.CreateAsync(CurrentUserId);
            return ok ? Ok(new { message }) : BadRequest(new { message });
        }
    }
}