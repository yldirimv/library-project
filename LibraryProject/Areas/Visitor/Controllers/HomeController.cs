using LibraryProject.Areas.Visitor.Controllers;
using LibraryProject.Business.Interfaces;
using LibraryProject.Model.Enums;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LibraryProject.Areas.Visitor.Controllers
{
    public class HomeController : VisitorBaseController
    {
        private readonly IReservationService _reservationService;
        private readonly ILoanService _loanService;

        public HomeController(IReservationService reservationService, ILoanService loanService)
        {
            _reservationService = reservationService;
            _loanService = loanService;
        }
            

        private string CurrentUserId
            => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        public async Task<IActionResult> Index()
        {
            ViewBag.ActiveLoans = (await _loanService.GetVisitorLoansAsync(CurrentUserId))
                .Where(l => l.Status == LoanStatus.Active).ToList();
            return View(await _reservationService.GetMyReservationsAsync(CurrentUserId));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var (ok, message) = await _reservationService.CancelAsync(id, CurrentUserId);
            TempData[ok ? "Success" : "Error"] = message;
            return RedirectToAction(nameof(Index));
        }
    }
}