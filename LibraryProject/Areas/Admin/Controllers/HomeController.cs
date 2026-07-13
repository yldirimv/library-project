using LibraryProject.Areas.Admin.Controllers;
using LibraryProject.Data.Queries;
using Microsoft.AspNetCore.Mvc;

namespace LibraryProject.Areas.Admin.Controllers
{
    public class HomeController : AdminBaseController
    {
        private readonly DashboardQueries _queries;

        public HomeController(DashboardQueries queries) => _queries = queries;

        public async Task<IActionResult> Index()
            => View(await _queries.GetAdminDashboardAsync());
    }
}
