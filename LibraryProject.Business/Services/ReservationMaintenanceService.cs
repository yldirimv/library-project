using LibraryProject.Business.Interfaces;
using LibraryProject.Model.Entities;
using LibraryProject.Model.Enums;
using LibraryProject.Model.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LibraryProject.Business.Services
{
    public class ReservationMaintenanceService : IReservationMaintenanceService
    {
        private readonly IUnitOfWork _uow;
        private readonly UserManager<AppUser> _userManager;
        private readonly ILogger<ReservationMaintenanceService> _logger;
        private readonly ISeatNotifier _seatNotifier;

        private const int LateToleranceMinutes = 30;
        private const int MaxBreakMinutes = 90;
        private const int BreakToleranceMinutes = 5;
        private const int NoShowBanThreshold = 3;
        private const int NoShowBanDays = 7;
        private const int PointsPerHour = 10;

        public ReservationMaintenanceService(IUnitOfWork uow,
            UserManager<AppUser> userManager,
            ILogger<ReservationMaintenanceService> logger,
            ISeatNotifier seatNotifier)
        {
            _uow = uow;
            _userManager = userManager;
            _logger = logger;
            _seatNotifier = seatNotifier;
        }

        public async Task RunAsync()
        {
            var now = DateTime.Now;
            await HandleNoShowsAsync(now);
            await HandleExpiredSessionsAsync(now);
            await HandleOverstayedBreaksAsync(now);
            await CleanExpiredTokensAsync(now);
        }

        //no show olunca iptal olması, 3 no showda yasak
        private async Task HandleNoShowsAsync(DateTime now)
        {
            var noShows = await _uow.Reservations.Query()
                .Where(r => r.Status == ReservationStatus.Pending
                         && r.StartTime.AddMinutes(LateToleranceMinutes) < now)
                .ToListAsync();

            foreach (var r in noShows)
            {
                r.Status = ReservationStatus.NoShow;
                _uow.Reservations.Update(r);

                var visitor = await _userManager.FindByIdAsync(r.VisitorId);
                if (visitor != null)
                {
                    visitor.NoShowCount++;
                    if (visitor.NoShowCount >= NoShowBanThreshold)
                    {
                        visitor.ReservationBanUntil = now.AddDays(NoShowBanDays);
                        visitor.NoShowCount = 0; //yasakla birlikte sayaç sıfırlanır
                        _logger.LogWarning(
                            "Ziyaretçi {Visitor} 3 no-show nedeniyle {Days} gün yasaklandı",
                            visitor.FullName, NoShowBanDays);
                    }
                    await _userManager.UpdateAsync(visitor);
                }

                _logger.LogInformation("Rezervasyon {Id} no-show nedeniyle iptal edildi", r.Id);
            }

            if (noShows.Any()) 
                await _uow.SaveChangesAsync();
                await _seatNotifier.NotifySeatChangedAsync();
        }

        //süre doldu hala bekleniyor veya moladaysa çıkıs yapar
        private async Task HandleExpiredSessionsAsync(DateTime now)
        {
            var expired = await _uow.Reservations.Query()
                .Include(r => r.Events)
                .Where(r => (r.Status == ReservationStatus.Active
                          || r.Status == ReservationStatus.OnBreak)
                         && r.EndTime < now)
                .ToListAsync();

            foreach (var r in expired)
            {
                r.Events.Add(new ReservationEvent
                {
                    EventType = ReservationEventType.CheckOut,
                    EventTime = r.EndTime // çıkışı rezervasyon bitişi say, şu anı değil
                });
                r.Status = ReservationStatus.Completed;
                _uow.Reservations.Update(r);

                var checkIn = r.Events
                    .Where(e => e.EventType == ReservationEventType.CheckIn)
                    .OrderBy(e => e.EventTime).FirstOrDefault();

                if (checkIn != null)
                {
                    var hours = (int)Math.Floor((r.EndTime - checkIn.EventTime).TotalHours);
                    if (hours > 0)
                    {
                        await _uow.LoyaltyTransactions.AddAsync(new LoyaltyTransaction
                        {
                            VisitorId = r.VisitorId,
                            Points = hours * PointsPerHour,
                            Source = LoyaltySource.SessionTime,
                            Description = $"{hours} saat çalışma (otomatik çıkış)"
                        });
                    }
                }

                _logger.LogInformation("Rezervasyon {Id} süre dolumu ile kapatıldı", r.Id);
            }

            if (expired.Any()) 
                await _uow.SaveChangesAsync();
                await _seatNotifier.NotifySeatChangedAsync();
        }

        //90+5 mola, aşılırsa rez iptal
        private async Task HandleOverstayedBreaksAsync(DateTime now)
        {
            var onBreak = await _uow.Reservations.Query()
                .Include(r => r.Events)
                .Where(r => r.Status == ReservationStatus.OnBreak)
                .ToListAsync();

            var cancelled = false;
            foreach (var r in onBreak)
            {
                var used = GetUsedBreakMinutes(r, now);
                if (used > MaxBreakMinutes + BreakToleranceMinutes)
                {
                    r.Status = ReservationStatus.Cancelled;
                    _uow.Reservations.Update(r);
                    cancelled = true;
                    _logger.LogInformation(
                        "Rezervasyon {Id} mola aşımı ({Minutes} dk) nedeniyle iptal edildi", r.Id, used);
                }
            }

            if (cancelled) 
                await _uow.SaveChangesAsync();
                await _seatNotifier.NotifySeatChangedAsync();
        }

        //1 saatten sonra suresi gecmis tokenları sil
        private async Task CleanExpiredTokensAsync(DateTime now)
        {
            var stale = await _uow.QrTokens
                .FindAsync(t => t.ExpiresAt < now.AddHours(-1));

            if (stale.Any())
            {
                foreach (var t in stale) _uow.QrTokens.Remove(t);
                await _uow.SaveChangesAsync();
            }
        }

        
        private int GetUsedBreakMinutes(Reservation r, DateTime now)
        {
            double total = 0;
            DateTime? open = null;

            foreach (var e in r.Events.OrderBy(e => e.EventTime))
            {
                if (e.EventType == ReservationEventType.BreakStart) open = e.EventTime;
                else if (e.EventType == ReservationEventType.BreakEnd && open.HasValue)
                { total += (e.EventTime - open.Value).TotalMinutes; open = null; }
            }

            if (open.HasValue) total += (now - open.Value).TotalMinutes;
            return (int)total;
        }
    }
}