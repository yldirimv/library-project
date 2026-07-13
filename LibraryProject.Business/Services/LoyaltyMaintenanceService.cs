using LibraryProject.Business.Interfaces;
using LibraryProject.Model.Entities;
using LibraryProject.Model.Enums;
using LibraryProject.Model.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LibraryProject.Business.Services
{
    public class LoyaltyMaintenanceService : ILoyaltyMaintenanceService
    {
        private readonly IUnitOfWork _uow;
        private readonly ILogger<LoyaltyMaintenanceService> _logger;

        private const int RequiredDistinctDays = 5;
        private const int WeeklyBonusPoints = 50;

        public LoyaltyMaintenanceService(IUnitOfWork uow,
            ILogger<LoyaltyMaintenanceService> logger)
        {
            _uow = uow;
            _logger = logger;
        }

        public async Task GrantWeeklyBonusesAsync()
        {
            //geçen haftanın aralığı: pazartesi 00:00 - bu pazartesi 00:00
            var today = DateTime.Today;
            int daysSinceMonday = ((int)today.DayOfWeek + 6) % 7; 
            var thisMonday = today.AddDays(-daysSinceMonday);
            var lastMonday = thisMonday.AddDays(-7);

            
            var checkIns = await _uow.ReservationEvents.Query()
                .Include(e => e.Reservation)
                .Where(e => e.EventType == ReservationEventType.CheckIn
                         && e.EventTime >= lastMonday
                         && e.EventTime < thisMonday)
                .Select(e => new { e.Reservation.VisitorId, Day = e.EventTime.Date })
                .Distinct()
                .ToListAsync();

            var qualified = checkIns
                .GroupBy(x => x.VisitorId)
                .Where(g => g.Count() >= RequiredDistinctDays)
                .Select(g => g.Key)
                .ToList();

            foreach (var visitorId in qualified)
            {
                // Aynı hafta için ikinci kez bonus yazma (job iki kez tetiklenirse)
                var marker = $"Haftalık bonus ({lastMonday:dd.MM.yyyy})";
                var already = await _uow.LoyaltyTransactions.FirstOrDefaultAsync(
                    t => t.VisitorId == visitorId && t.Description == marker);
                if (already != null) continue;

                await _uow.LoyaltyTransactions.AddAsync(new LoyaltyTransaction
                {
                    VisitorId = visitorId,
                    Points = WeeklyBonusPoints,
                    Source = LoyaltySource.WeeklyBonus,
                    Description = marker
                });

                _logger.LogInformation("Ziyaretçi {Id} haftalık bonus kazandı", visitorId);
            }

            if (qualified.Any()) await _uow.SaveChangesAsync();
        }
    }
}