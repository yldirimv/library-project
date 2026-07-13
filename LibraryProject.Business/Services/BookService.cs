using LibraryProject.Business.Interfaces;
using LibraryProject.Model.Entities;
using LibraryProject.Model.Enums;
using LibraryProject.Model.Interfaces;

namespace LibraryProject.Business.Services
{
    public class BookService : IBookService
    {
        private readonly IUnitOfWork _uow;

        public BookService(IUnitOfWork uow) => _uow = uow;

        public async Task<List<Book>> GetAllAsync(string? search = null)
        {
            var books = string.IsNullOrWhiteSpace(search)
                ? await _uow.Books.FindAsync(b => b.IsActive)
                : await _uow.Books.FindAsync(b => b.IsActive && b.Title.Contains(search)); //like %..%

            return books.OrderBy(b => b.Title).ToList();
        }

        public async Task<Book?> GetByIdAsync(int id)
            => await _uow.Books.GetByIdAsync(id);

        public async Task<int> GetActiveLoanCountAsync(int bookId)
            => (await _uow.BookLoans.FindAsync(
                    l => l.BookId == bookId && l.Status == LoanStatus.Active)).Count;

        public async Task CreateAsync(Book book)
        {
            await _uow.Books.AddAsync(book);
            await _uow.SaveChangesAsync();
        }

        public async Task UpdateAsync(Book book)
        {
            _uow.Books.Update(book);
            await _uow.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var book = await _uow.Books.GetByIdAsync(id);
            if (book == null) return;

            book.IsActive = false;
            _uow.Books.Update(book);
            await _uow.SaveChangesAsync();
        }
    }
}