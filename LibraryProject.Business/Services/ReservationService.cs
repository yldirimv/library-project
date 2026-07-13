using LibraryProject.Business.Interfaces;
using LibraryProject.Model.Dtos;
using LibraryProject.Model.Entities;
using LibraryProject.Model.Enums;
using LibraryProject.Model.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LibraryProject.Business.Services
{
    public class ReservationService : IReservationService
    {
        private readonly IUnitOfWork _uow;
        private readonly UserManager<AppUser> _userManager;
        private const int MaxActiveReservations = 3;
        private const int MaxDurationHours = 6;

        //koltugun mesgul olduğu durumlar. çakışma hep bunlara bakacak
        private static readonly ReservationStatus[] BlockingStatuses =
        {
            ReservationStatus.Pending,
            ReservationStatus.Active,
            ReservationStatus.OnBreak
        };

        public ReservationService(IUnitOfWork uow, UserManager<AppUser> userManager)
        {
            _uow = uow;
            _userManager = userManager;
        }

        public async Task<List<Reservation>> GetMyReservationsAsync(string visitorId)
            => await _uow.Reservations.Query()
                .Include(r => r.Seat)
                .Where(r => r.VisitorId == visitorId)
                .OrderByDescending(r => r.StartTime) 
                .ToListAsync();

        public async Task<List<Seat>> GetSeatsAsync(int floor)
            => (await _uow.Seats.FindAsync(s => s.IsActive && s.Floor == floor))
                .OrderBy(s => s.SeatNumber).ToList();

        //krokide boyanacak olanlar yani dolu koltuklar
        public async Task<List<int>> GetOccupiedSeatIdsAsync(DateTime start, DateTime end)
            => await _uow.Reservations.Query()
                .Where(r => BlockingStatuses.Contains(r.Status)
                         && r.StartTime < end
                         && r.EndTime > start)
                .Select(r => r.SeatId)
                .Distinct()
                .ToListAsync();

        public async Task<(bool ok, string message)> CreateAsync(
            string visitorId, int seatId, DateTime start, DateTime end)
        {
            //yasaklı mı kontrolü
            var visitor = await _userManager.FindByIdAsync(visitorId);
            if (visitor == null) return (false, "Kullanıcı bulunamadı.");

            if (visitor.ReservationBanUntil.HasValue &&
                visitor.ReservationBanUntil.Value > DateTime.Now)
                return (false, $"Rezervasyon yasağınız var. Bitiş: " +
                               $"{visitor.ReservationBanUntil.Value:dd.MM.yyyy HH:mm}");

            //rez zamanı uygun mu kontrolü
            if (start < DateTime.Now)
                return (false, "Geçmiş bir saate rezervasyon yapılamaz.");

            if (end <= start)
                return (false, "Bitiş saati başlangıçtan sonra olmalıdır.");

            if ((end - start).TotalHours > MaxDurationHours)
                return (false, $"Bir rezervasyon en fazla {MaxDurationHours} saat olabilir.");

            //tek seferde alınabilecek rez limiti kontrolü
            var activeCount = (await _uow.Reservations.FindAsync(
                r => r.VisitorId == visitorId && BlockingStatuses.Contains(r.Status))).Count;

            if (activeCount >= MaxActiveReservations)
                return (false, $"Aynı anda en fazla {MaxActiveReservations} aktif rezervasyonunuz olabilir.");

            //koltuk bos mu dolu mu kontrolü
            var conflict = await _uow.Reservations.Query()
                .AnyAsync(r => r.SeatId == seatId
                            && BlockingStatuses.Contains(r.Status)
                            && r.StartTime < end
                            && r.EndTime > start);

            if (conflict)
                return (false, "Bu koltuk seçtiğiniz saat aralığında dolu.");

            //tüm kuralları gectiyse rez yapılır
            await _uow.Reservations.AddAsync(new Reservation
            {
                VisitorId = visitorId,
                SeatId = seatId,
                StartTime = start,
                EndTime = end,
                Status = ReservationStatus.Pending
            });

            await _uow.SaveChangesAsync(); //hepsinden emin oluyor sagolsun
            return (true, "Rezervasyon oluşturuldu.");
        }

        public async Task<(bool ok, string message)> CancelAsync(int reservationId, string visitorId)
        {
            var reservation = await _uow.Reservations.GetByIdAsync(reservationId);

            if (reservation == null)
                return (false, "Rezervasyon bulunamadı.");

            //güvenlik=> başkasının rezervasyonunu iptal edemezsin
            if (reservation.VisitorId != visitorId)
                return (false, "Bu rezervasyon size ait değil.");

            if (reservation.Status != ReservationStatus.Pending)
                return (false, "Sadece henüz başlamamış rezervasyonlar iptal edilebilir.");

            reservation.Status = ReservationStatus.Cancelled;
            _uow.Reservations.Update(reservation);
            await _uow.SaveChangesAsync();

            return (true, "Rezervasyon iptal edildi.");
        }

        // Şu ANDA aktif olan rezervasyonlara göre koltuk -> durum sözlüğü
        // (kroki partial'ının beklediği CSS sınıf adlarıyla: reserved/occupied/break)
        public async Task<Dictionary<int, SeatStateDto>> GetCurrentSeatStatesAsync()
        {
            var now = DateTime.Now;

            var current = await _uow.Reservations.Query()
                .Include(r => r.Visitor)
                .Where(r => BlockingStatuses.Contains(r.Status)
                         && r.StartTime <= now.AddMinutes(30)
                         && r.EndTime > now)
                .ToListAsync();

            return current.ToDictionary(
                r => r.SeatId,
                r => new SeatStateDto
                {
                    State = r.Status switch
                    {
                        ReservationStatus.Active => "occupied",
                        ReservationStatus.OnBreak => "break",
                        _ => "reserved"
                    },
                    VisitorName = r.Visitor.FullName,
                    TimeRange = $"{r.StartTime:HH:mm} - {r.EndTime:HH:mm}"
                });
        }

        public async Task<List<Reservation>> GetTodaysReservationsAsync()
        {
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);

            return await _uow.Reservations.Query()
                .Include(r => r.Visitor)
                .Include(r => r.Seat)
                .Include(r => r.Events)
                .Where(r => r.StartTime >= today && r.StartTime < tomorrow)
                .OrderBy(r => r.StartTime)
                .ToListAsync();
        }
    }
}