using LibraryProject.Areas.Visitor.Controllers;
using LibraryProject.Business.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LibraryProject.Web.Areas.Visitor.Controllers
{
    public class MyBooksController : VisitorBaseController
    {
        private readonly ILoanService _loanService;

        public MyBooksController(ILoanService loanService) => _loanService = loanService;

        public async Task<IActionResult> Index()
        {
            var visitorId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            return View(await _loanService.GetVisitorLoansAsync(visitorId));
        }
    }
}