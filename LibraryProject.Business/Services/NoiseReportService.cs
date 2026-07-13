using LibraryProject.Business.Interfaces;
using LibraryProject.Model.Entities;
using LibraryProject.Model.Enums;
using LibraryProject.Model.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LibraryProject.Business.Services
{
    public class NoiseReportService : INoiseReportService
    {
        private readonly IUnitOfWork _uow;

        public NoiseReportService(IUnitOfWork uow) => _uow = uow;

        public async Task<(bool ok, string message, NoiseReport? report)> CreateAsync(string visitorId)
        {
            var current = await _uow.Reservations.Query()
    .Include(r => r.Seat)
    .FirstOrDefaultAsync(r => r.VisitorId == visitorId
        && r.Status == ReservationStatus.Active);   // sadece içeride olan

            if (current == null)
                return (false, "İhbar için koltuğunuzda oturuyor olmalısınız.", null);

            var report = new NoiseReport
            {
                VisitorId = visitorId,
                SeatId = current.SeatId,
                Status = NoiseReportStatus.New
            };

            await _uow.NoiseReports.AddAsync(report);
            await _uow.SaveChangesAsync();

            report.Seat = current.Seat; // hub mesajında koltuk no lazım
            return (true, "İhbarınız iletildi. Görevli en kısa sürede ilgilenecek.", report);
        }

        public async Task<List<NoiseReport>> GetOpenReportsAsync()
            => await _uow.NoiseReports.Query()
                .Include(n => n.Seat)
                .Where(n => n.Status == NoiseReportStatus.New)
                .OrderBy(n => n.CreatedDate)
                .ToListAsync();

        public async Task MarkHandledAsync(int reportId)
        {
            var report = await _uow.NoiseReports.GetByIdAsync(reportId);
            if (report == null) return;
            report.Status = NoiseReportStatus.Handled;
            _uow.NoiseReports.Update(report);
            await _uow.SaveChangesAsync();
        }
    }
}