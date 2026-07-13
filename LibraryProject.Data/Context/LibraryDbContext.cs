using LibraryProject.Model.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LibraryProject.Data.Context
{
    public class LibraryDbContext : IdentityDbContext<AppUser> //normal dbcontext değil, identity tablolarını otomatik kurcak
        
    {
        public LibraryDbContext(DbContextOptions<LibraryDbContext> options)
            : base(options) { }

        public DbSet<Seat> Seats { get; set; }//tablolarım...base ve user icin yok base abstract, user da identity'nin
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<ReservationEvent> ReservationEvents { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<BookLoan> BookLoans { get; set; }
        public DbSet<LoyaltyTransaction> LoyaltyTransactions { get; set; }
        public DbSet<Gift> Gifts { get; set; }
        public DbSet<GiftRequest> GiftRequests { get; set; }
        public DbSet<Announcement> Announcements { get; set; }
        public DbSet<NoiseReport> NoiseReports { get; set; }
        public DbSet<QrToken> QrTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder); // identity tablolarının konfigurasyonunu calıstırır

            // BookLoan'daki çift AppUser ilişkisinin elle tarifi FLUENT API
            //ef nin isim kuralıyla iliskiyi çozemediği durumlarda yardımcı.


            builder.Entity<BookLoan>()//bookloan tablosu icin
                .HasOne(l => l.Visitor)//her bookloan'ın bir visitor'u var
                .WithMany(u => u.Loans)//bir visitorun çok loanı var
                .HasForeignKey(l => l.VisitorId)//baglantıyı visitorid tutuyor
                .OnDelete(DeleteBehavior.Restrict);//silmeyi kısıtla

            builder.Entity<BookLoan>()
                .HasOne(l => l.Employee)
                .WithMany()
                .HasForeignKey(l => l.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);//cascade yani zincirleme silme
            //ana kayıt silinirse ona baglı tüm kayıtlar da silinecek
            //restrict ise buna izin vermez. gecmis kaydı tutmak istiyoruz cunku. soft delete yapma nedenimiz bu
            //bir de sqlserver da multiple cascade paths hatası almamak icin rstrict yaptık

            
            builder.Entity<Reservation>()
                .HasOne(r => r.Visitor)
                .WithMany(u => u.Reservations)
                .HasForeignKey(r => r.VisitorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<NoiseReport>()
                .HasOne(n => n.Visitor)
                .WithMany()
                .HasForeignKey(n => n.VisitorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<NoiseReport>()
                .HasOne(n => n.Seat)
                .WithMany()
                .HasForeignKey(n => n.SeatId)
                .OnDelete(DeleteBehavior.Restrict);

            // Sık sorgulanacak alanlara index — kroki ve çakışma kontrolü hep buradan okuyacak
            builder.Entity<Reservation>()
                .HasIndex(r => new { r.SeatId, r.StartTime });

            builder.Entity<QrToken>()
                .HasIndex(q => q.Token);
        }
    }
}