using LibraryProject.Business.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LibraryProject.Api.Controllers
{
    [Route("api/announcements")]
    public class AnnouncementsController : ApiBaseController
    {
        private readonly IAnnouncementService _announcementService;

        public AnnouncementsController(IAnnouncementService announcementService)
            => _announcementService = announcementService;

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var announcements = await _announcementService.GetPublishedAsync();
            return Ok(announcements.Select(a => new
            {
                a.Id,
                a.Title,
                a.Content,
                date = a.CreatedDate
            }));
        }
    }
}