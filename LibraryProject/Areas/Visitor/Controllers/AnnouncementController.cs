using LibraryProject.Areas.Visitor.Controllers;
using LibraryProject.Business.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LibraryProject.Areas.Visitor.Controllers
{
    public class AnnouncementController : VisitorBaseController
    {
        private readonly IAnnouncementService _announcementService;

        public AnnouncementController(IAnnouncementService announcementService)
            => _announcementService = announcementService;

        public async Task<IActionResult> Index()
            => View(await _announcementService.GetPublishedAsync());
    }
}