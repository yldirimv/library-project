using LibraryProject.Model.Entities;

namespace LibraryProject.Business.Interfaces
{
    public interface IBookService
    {
        Task<List<Book>> GetAllAsync(string? search = null);
        Task<Book?> GetByIdAsync(int id);
        Task<int> GetActiveLoanCountAsync(int bookId);   // stok 
        Task CreateAsync(Book book);
        Task UpdateAsync(Book book);
        Task DeleteAsync(int id);
    }
}