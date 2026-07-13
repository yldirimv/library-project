using LibraryProject.Model.Entities;

namespace LibraryProject.Business.Interfaces
{
    public interface ILoanService
    {
        Task<List<BookLoan>> GetActiveLoansAsync(string? search = null);
        Task<(bool ok, string message)> LendAsync(string visitorTc, int bookId, string employeeId);
        Task<(bool ok, string message)> ReturnAsync(int loanId);
        Task<List<BookLoan>> GetVisitorLoansAsync(string visitorId);
    }
}