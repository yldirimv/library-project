using System.Linq.Expressions;

namespace LibraryProject.Model.Interfaces
{
    public interface IRepository<T> where T : class
    {
        //metot sözleşmeleri, rolleri
        Task<T?> GetByIdAsync(int id);//id si ... olan kaydı getir
        Task<List<T>> GetAllAsync();//tablonun tamamını getir
        Task<List<T>> FindAsync(Expression<Func<T, bool>> predicate);//koşula uyanları getir ada göre arama gibi
        
        // expression linq daki where kosulu gibi, kosul ister
        Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);//koşula uyan ilk kayıt ya da null
        Task AddAsync(T entity);//yeni kayıt ekle

        //bunlar task değil çünkü sadece isaret koyarlar, gercek is savechanges da olur
        void Update(T entity);//değiştir
        void Remove(T entity);//sil
        IQueryable<T> Query(); //IQueryable döndürür, include gerektiren birlesik sorgular icin
        //rez koltuğunu ve sahibini birlikte cekebilmek icin
    }
}