using System.Linq.Expressions;
using LibraryProject.Data.Context;
using LibraryProject.Model.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LibraryProject.Data.Repositories
{
    //IRepository sözleşmelerinin ef ile doldurulmus hali. 
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly LibraryDbContext _context;
        private readonly DbSet<T> _dbSet;

        public Repository(LibraryDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>(); // T neyse onun tablosunu bana bagla der
        }

        public async Task<T?> GetByIdAsync(int id) => await _dbSet.FindAsync(id);

        public async Task<List<T>> GetAllAsync() => await _dbSet.ToListAsync();

        public async Task<List<T>> FindAsync(Expression<Func<T, bool>> predicate)
            => await _dbSet.Where(predicate).ToListAsync();

        public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
            => await _dbSet.FirstOrDefaultAsync(predicate);

        public async Task AddAsync(T entity) => await _dbSet.AddAsync(entity);

        public void Update(T entity) => _dbSet.Update(entity);

        public void Remove(T entity) => _dbSet.Remove(entity);

        public IQueryable<T> Query() => _dbSet.AsQueryable();
    }
}