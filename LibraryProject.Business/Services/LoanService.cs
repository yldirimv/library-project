using LibraryProject.Business.Interfaces;
using LibraryProject.Model.Entities;
using LibraryProject.Model.Enums;
using LibraryProject.Model.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LibraryProject.Business.Services
{
    public class LoanService : ILoanService
    {
        private readonly IUnitOfWork _uow;
        private readonly UserManager<AppUser> _userManager;

        private const int MaxActiveLoans = 5;
        private const int LoanDurationDays = 20;
        private const int LateBanDays = 30;

        public LoanService(IUnitOfWork uow, UserManager<AppUser> userManager)
        {
            _uow = uow;
            _userManager = userManager;
        }

        public async Task<List<BookLoan>> GetActiveLoansAsync(string? search = null)
        {
            var query = _uow.BookLoans.Query()
                .Include(l => l.Book)
                .Include(l => l.Visitor)
                .Where(l => l.Status == LoanStatus.Active);

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(l => l.Visitor.FullName.Contains(search)
                                      || l.Book.Title.Contains(search));

            return await query.OrderBy(l => l.DueDate).ToListAsync();
        }

        public async Task<(bool ok, string message)> LendAsync(
            string visitorTc, int bookId, string employeeId)
        {
            //tc bul
            var visitor = _userManager.Users
                .FirstOrDefault(u => u.IdentityNumber == visitorTc);
            if (visitor == null)
                return (false, "Bu TC ile kayıtlı ziyaretçi bulunamadı.");

            //yasaklı mı kntrolu
            if (visitor.LoanBanUntil.HasValue && visitor.LoanBanUntil.Value > DateTime.Now)
                return (false, $"Ziyaretçinin kitap alma yasağı var. " +
                               $"Bitiş: {visitor.LoanBanUntil.Value:dd.MM.yyyy}");

            //5 kitap limiti
            var activeCount = (await _uow.BookLoans.FindAsync(
                l => l.VisitorId == visitor.Id && l.Status == LoanStatus.Active)).Count;
            if (activeCount >= MaxActiveLoans)
                return (false, $"Ziyaretçinin üzerinde zaten {MaxActiveLoans} kitap var.");

            //kitap ve stok kontrolu
            var book = await _uow.Books.GetByIdAsync(bookId);
            if (book == null || !book.IsActive)
                return (false, "Kitap bulunamadı.");

            var loanedOut = (await _uow.BookLoans.FindAsync(
                l => l.BookId == bookId && l.Status == LoanStatus.Active)).Count;
            if (loanedOut >= book.TotalStock)
                return (false, "Bu kitabın tüm kopyaları ödünçte.");

            //kaydet
            await _uow.BookLoans.AddAsync(new BookLoan
            {
                BookId = bookId,
                VisitorId = visitor.Id,
                EmployeeId = employeeId,
                LoanDate = DateTime.Now,
                DueDate = DateTime.Now.AddDays(LoanDurationDays),
                Status = LoanStatus.Active
            });
            await _uow.SaveChangesAsync();

            return (true, $"{book.Title} → {visitor.FullName}. " +
                          $"Son teslim: {DateTime.Now.AddDays(LoanDurationDays):dd.MM.yyyy}");
        }

        public async Task<(bool ok, string message)> ReturnAsync(int loanId)
        {
            var loan = await _uow.BookLoans.Query()
                .Include(l => l.Book)
                .FirstOrDefaultAsync(l => l.Id == loanId);

            if (loan == null || loan.Status != LoanStatus.Active)
                return (false, "Aktif ödünç kaydı bulunamadı.");

            loan.ReturnDate = DateTime.Now;
            loan.Status = LoanStatus.Returned;

            var message = $"{loan.Book.Title} iade alındı.";

            //gec iadede yasak
            if (DateTime.Now > loan.DueDate)
            {
                var visitor = await _userManager.FindByIdAsync(loan.VisitorId);
                if (visitor != null)
                {
                    visitor.LoanBanUntil = DateTime.Now.AddDays(LateBanDays);
                    await _userManager.UpdateAsync(visitor);
                    message += $" Geç iade nedeniyle ziyaretçiye {LateBanDays} gün kitap alma yasağı uygulandı.";
                }
            }

            _uow.BookLoans.Update(loan);
            await _uow.SaveChangesAsync();

            return (true, message);
        }

        public async Task<List<BookLoan>> GetVisitorLoansAsync(string visitorId)
            => await _uow.BookLoans.Query()
                .Include(l => l.Book)
                .Where(l => l.VisitorId == visitorId)
                .OrderByDescending(l => l.LoanDate)
                .ToListAsync();
    }
}