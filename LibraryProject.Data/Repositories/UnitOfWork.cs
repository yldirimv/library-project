using LibraryProject.Data.Context;
using LibraryProject.Model.Entities;
using LibraryProject.Model.Interfaces;

namespace LibraryProject.Data.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly LibraryDbContext _context; //tek bir dbcontext alır ve saklar
        //ya hep ya hic burada calısır. farklı tablolarda yapılan degisiklikler save changes ile ya birlikte kaydedilir ya da hiç kaydedilmez

        //Lazy: repository'ler ilk istendiğinde kurulur
        private IRepository<Seat>? _seats;
        private IRepository<Reservation>? _reservations;
        private IRepository<ReservationEvent>? _reservationEvents;
        private IRepository<Book>? _books;
        private IRepository<BookLoan>? _bookLoans;
        private IRepository<LoyaltyTransaction>? _loyaltyTransactions;
        private IRepository<Gift>? _gifts;
        private IRepository<GiftRequest>? _giftRequests;
        private IRepository<Announcement>? _announcements;
        private IRepository<NoiseReport>? _noiseReports;
        private IRepository<QrToken>? _qrTokens;

        public UnitOfWork(LibraryDbContext context) => _context = context;

        //??= operatörü daha once kurduysam eskisini ver kurmadıysam şimdi kur demektir. bosuna nesne uretmez
        public IRepository<Seat> Seats => _seats ??= new Repository<Seat>(_context);
        public IRepository<Reservation> Reservations => _reservations ??= new Repository<Reservation>(_context);
        public IRepository<ReservationEvent> ReservationEvents => _reservationEvents ??= new Repository<ReservationEvent>(_context);
        public IRepository<Book> Books => _books ??= new Repository<Book>(_context);
        public IRepository<BookLoan> BookLoans => _bookLoans ??= new Repository<BookLoan>(_context);
        public IRepository<LoyaltyTransaction> LoyaltyTransactions => _loyaltyTransactions ??= new Repository<LoyaltyTransaction>(_context);
        public IRepository<Gift> Gifts => _gifts ??= new Repository<Gift>(_context);
        public IRepository<GiftRequest> GiftRequests => _giftRequests ??= new Repository<GiftRequest>(_context);
        public IRepository<Announcement> Announcements => _announcements ??= new Repository<Announcement>(_context);
        public IRepository<NoiseReport> NoiseReports => _noiseReports ??= new Repository<NoiseReport>(_context);
        public IRepository<QrToken> QrTokens => _qrTokens ??= new Repository<QrToken>(_context);

        public async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();//tek seferde tek transactıon da veritababnına kaydeder

        public void Dispose() => _context.Dispose();
    }
}