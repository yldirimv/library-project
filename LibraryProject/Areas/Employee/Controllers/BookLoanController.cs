using LibraryProject.Areas.Employee.Controllers;
using LibraryProject.Business.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LibraryProject.Web.Areas.Employee.Controllers
{
    public class BookLoanController : EmployeeBaseController
    {
        private readonly ILoanService _loanService;
        private readonly IBookService _bookService;

        public BookLoanController(ILoanService loanService, IBookService bookService)
        {
            _loanService = loanService;
            _bookService = bookService;
        }

        public async Task<IActionResult> Index(string? search)
        {
            ViewBag.Search = search;
            return View(await _loanService.GetActiveLoansAsync(search));
        }

        public async Task<IActionResult> Lend()
        {
            ViewBag.Books = await _bookService.GetAllAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Lend(string visitorTc, int bookId)
        {
            var employeeId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var (ok, message) = await _loanService.LendAsync(visitorTc, bookId, employeeId);

            TempData[ok ? "Success" : "Error"] = message;
            return ok ? RedirectToAction(nameof(Index)) : RedirectToAction(nameof(Lend));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Return(int id)
        {
            var (ok, message) = await _loanService.ReturnAsync(id);
            TempData[ok ? "Success" : "Error"] = message;
            return RedirectToAction(nameof(Index));
        }
    }
}