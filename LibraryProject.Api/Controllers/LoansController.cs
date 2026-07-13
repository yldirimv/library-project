using LibraryProject.Business.Interfaces;
using LibraryProject.Model.Enums;
using Microsoft.AspNetCore.Mvc;

namespace LibraryProject.Api.Controllers
{
    [Route("api/loans")]
    public class LoansController : ApiBaseController
    {
        private readonly ILoanService _loanService;

        public LoansController(ILoanService loanService) => _loanService = loanService;

        [HttpGet("mine")]
        public async Task<IActionResult> Mine()
        {
            var loans = await _loanService.GetVisitorLoansAsync(CurrentUserId);

            return Ok(loans.Select(l => new
            {
                l.Id,
                title = l.Book.Title,
                author = l.Book.Author,
                loanDate = l.LoanDate,
                dueDate = l.DueDate,
                returned = l.Status == LoanStatus.Returned,
                overdue = l.Status == LoanStatus.Active && DateTime.Now > l.DueDate
            }));
        }
    }
}