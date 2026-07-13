using LibraryProject.Areas.Employee.Controllers;
using LibraryProject.Business.Interfaces;
using LibraryProject.Data.Queries;
using Microsoft.AspNetCore.Mvc;

namespace LibraryProject.Web.Areas.Employee.Controllers
{
    public class HomeController : EmployeeBaseController
    {
        private readonly DashboardQueries _queries;
        private readonly INoiseReportService _noiseService;

        public HomeController(DashboardQueries queries, INoiseReportService noiseService)
        {
            _queries = queries;
            _noiseService = noiseService;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.OpenReports = await _noiseService.GetOpenReportsAsync();
            return View(await _queries.GetAdminDashboardAsync());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Handled(int id)
        {
            await _noiseService.MarkHandledAsync(id);
            TempData["Success"] = "İhbar kapatıldı.";
            return RedirectToAction(nameof(Index));
        }
    }
}