using LibraryProject.Business.Interfaces;
using LibraryProject.Model.Entities;
using LibraryProject.Model.Enums;
using LibraryProject.Model.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LibraryProject.Business.Services
{
    public class CheckInService : ICheckInService
    {
        private readonly IUnitOfWork _uow;
        private readonly IQrService _qrService;
        private readonly ISeatNotifier _seatNotifier;

        private const int LateToleranceMinutes = 30;   // girişe geç kalma payı
        private const int MaxBreakMinutes = 90;        // toplam mola hakkı
        private const int BreakToleranceMinutes = 5;   // mola aşım toleransı
        private const int PointsPerHour = 10;          // saat başı puan

        public CheckInService(IUnitOfWork uow, IQrService qrService, ISeatNotifier seatNotifier)
        {
            _uow = uow;
            _qrService = qrService;
            _seatNotifier = seatNotifier;
        }

        public async Task<(bool ok, string message)> ProcessAsync(
            string visitorId, string qrToken, ReservationEventType action)
        {
            // 1) Token geçerli mi?
            var (valid, tokenMessage) = await _qrService.ValidateAndConsumeAsync(qrToken);
            if (!valid) return (false, tokenMessage);

            // 2) İşleme uygun rezervasyonu bul
            var now = DateTime.Now;

            var reservation = action == ReservationEventType.CheckIn
                ? await _uow.Reservations.Query()
                    .Include(r => r.Events)
                    .Include(r => r.Seat)
                    .FirstOrDefaultAsync(r => r.VisitorId == visitorId
                        && r.Status == ReservationStatus.Pending
                        && r.StartTime <= now.AddMinutes(LateToleranceMinutes) // erken gelme dahil
                        && r.EndTime > now)
                : await _uow.Reservations.Query()
                    .Include(r => r.Events)
                    .Include(r => r.Seat)
                    .FirstOrDefaultAsync(r => r.VisitorId == visitorId
                        && (r.Status == ReservationStatus.Active
                         || r.Status == ReservationStatus.OnBreak));

            if (reservation == null)
                return (false, action == ReservationEventType.CheckIn
                    ? "Şu an giriş yapabileceğiniz bir rezervasyon bulunamadı."
                    : "Aktif bir oturumunuz bulunamadı.");

            // 3) İşleme göre kuralları işlet
            return action switch
            {
                ReservationEventType.CheckIn => await HandleCheckInAsync(reservation, now),
                ReservationEventType.BreakStart => await HandleBreakStartAsync(reservation, now),
                ReservationEventType.BreakEnd => await HandleBreakEndAsync(reservation, now),
                ReservationEventType.CheckOut => await HandleCheckOutAsync(reservation, now),
                _ => (false, "Geçersiz işlem.")
            };
        }

        // ---------- GİRİŞ ----------
        private async Task<(bool, string)> HandleCheckInAsync(Reservation r, DateTime now)
        {
            if (now > r.StartTime.AddMinutes(LateToleranceMinutes))
                return (false, "Giriş süreniz geçmiş."); // güvenlik ağı; sorgu zaten eliyor

            AddEvent(r, ReservationEventType.CheckIn, now);
            r.Status = ReservationStatus.Active;
            _uow.Reservations.Update(r);
            await _uow.SaveChangesAsync();
            await _seatNotifier.NotifySeatChangedAsync();
            return (true, $"Giriş yapıldı. İyi çalışmalar! Koltuk: {r.Seat.SeatNumber}");
        }

        // ---------- MOLA BAŞLAT ----------
        private async Task<(bool, string)> HandleBreakStartAsync(Reservation r, DateTime now)
        {
            if (r.Status != ReservationStatus.Active)
                return (false, "Mola vermek için içeride olmalısınız.");

            var used = GetUsedBreakMinutes(r, now);
            if (used >= MaxBreakMinutes)
                return (false, "Mola hakkınız tükendi.");

            AddEvent(r, ReservationEventType.BreakStart, now);
            r.Status = ReservationStatus.OnBreak;
            _uow.Reservations.Update(r);
            await _uow.SaveChangesAsync();
            await _seatNotifier.NotifySeatChangedAsync();
            var remaining = MaxBreakMinutes - used;
            return (true, $"Mola başladı. Kalan mola hakkınız: {remaining} dk.");
        }

        // ---------- MOLA BİTİR ----------
        private async Task<(bool, string)> HandleBreakEndAsync(Reservation r, DateTime now)
        {
            if (r.Status != ReservationStatus.OnBreak)
                return (false, "Molada görünmüyorsunuz.");

            var used = GetUsedBreakMinutes(r, now); // şu ana kadarki toplam (açık mola dahil)

            if (used > MaxBreakMinutes + BreakToleranceMinutes)
            {
                // Tolerans da aşıldı: rezervasyon yanar
                AddEvent(r, ReservationEventType.BreakEnd, now);
                r.Status = ReservationStatus.Cancelled;
                _uow.Reservations.Update(r);
                await _uow.SaveChangesAsync();
                await _seatNotifier.NotifySeatChangedAsync();
                return (false, "Mola süreniz aşıldığı için rezervasyonunuz iptal edildi.");
            }

            AddEvent(r, ReservationEventType.BreakEnd, now);
            r.Status = ReservationStatus.Active;
            _uow.Reservations.Update(r);
            await _uow.SaveChangesAsync();
            await _seatNotifier.NotifySeatChangedAsync();
            return (true, "Tekrar hoş geldiniz. İyi çalışmalar!");
        }

        // ---------- ÇIKIŞ ----------
        private async Task<(bool, string)> HandleCheckOutAsync(Reservation r, DateTime now)
        {
            AddEvent(r, ReservationEventType.CheckOut, now);
            r.Status = ReservationStatus.Completed;
            _uow.Reservations.Update(r);

            // Puan: giriş-çıkış arası, saat başı PointsPerHour
            var checkIn = r.Events
                .Where(e => e.EventType == ReservationEventType.CheckIn)
                .OrderBy(e => e.EventTime)
                .FirstOrDefault();

            int points = 0;
            if (checkIn != null)
            {
                var hours = (int)Math.Floor((now - checkIn.EventTime).TotalHours);
                points = hours * PointsPerHour;

                if (points > 0)
                {
                    await _uow.LoyaltyTransactions.AddAsync(new LoyaltyTransaction
                    {
                        VisitorId = r.VisitorId,
                        Points = points,
                        Source = LoyaltySource.SessionTime,
                        Description = $"{hours} saat çalışma"
                    });
                }
            }

            await _uow.SaveChangesAsync(); // event + status + puan: tek transaction
            await _seatNotifier.NotifySeatChangedAsync();
            return (true, points > 0
                ? $"Çıkış yapıldı. {points} puan kazandınız!"
                : "Çıkış yapıldı. İyi günler!");
        }

        // ---------- YARDIMCILAR ----------
        private void AddEvent(Reservation r, ReservationEventType type, DateTime time)
            => r.Events.Add(new ReservationEvent { EventType = type, EventTime = time });

        // Parçalı mola toplamı: BreakStart-BreakEnd çiftlerini eşleştir,
        // kapanmamış mola varsa "şu ana kadar"ı say
        private int GetUsedBreakMinutes(Reservation r, DateTime now)
        {
            double total = 0;
            DateTime? openBreak = null;

            foreach (var e in r.Events.OrderBy(e => e.EventTime))
            {
                if (e.EventType == ReservationEventType.BreakStart)
                    openBreak = e.EventTime;
                else if (e.EventType == ReservationEventType.BreakEnd && openBreak.HasValue)
                {
                    total += (e.EventTime - openBreak.Value).TotalMinutes;
                    openBreak = null;
                }
            }

            if (openBreak.HasValue) // hâlâ molada: açık molayı da say
                total += (now - openBreak.Value).TotalMinutes;

            return (int)total;
        }
    }
}