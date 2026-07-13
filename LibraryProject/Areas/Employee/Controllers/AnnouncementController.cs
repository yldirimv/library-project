using LibraryProject.Areas.Employee.Controllers;
using LibraryProject.Business.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LibraryProject.Areas.Employee.Controllers
{
    public class AnnouncementController : EmployeeBaseController
    {
        private readonly IAnnouncementService _announcementService;

        public AnnouncementController(IAnnouncementService announcementService)
            => _announcementService = announcementService;

        public async Task<IActionResult> Index()
            => View(await _announcementService.GetPublishedAsync());
    }
}