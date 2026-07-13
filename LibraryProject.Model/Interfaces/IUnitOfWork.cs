using LibraryProject.Model.Entities;

namespace LibraryProject.Model.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        //servisler sadece bunu tanır
        //disposable dan türemesi isi bitince contexti kapatması gerektigi için
        IRepository<Seat> Seats { get; }
        IRepository<Reservation> Reservations { get; }
        IRepository<ReservationEvent> ReservationEvents { get; }
        IRepository<Book> Books { get; }
        IRepository<BookLoan> BookLoans { get; }
        IRepository<LoyaltyTransaction> LoyaltyTransactions { get; }
        IRepository<Gift> Gifts { get; }
        IRepository<GiftRequest> GiftRequests { get; }
        IRepository<Announcement> Announcements { get; }
        IRepository<NoiseReport> NoiseReports { get; }
        IRepository<QrToken> QrTokens { get; }

        Task<int> SaveChangesAsync();
    }
}