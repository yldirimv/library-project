using LibraryProject.Business.Interfaces;
using LibraryProject.Model.Entities;
using LibraryProject.Model.Enums;
using LibraryProject.Model.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LibraryProject.Business.Services
{
    public class LoyaltyService : ILoyaltyService
    {
        private readonly IUnitOfWork _uow;

        public LoyaltyService(IUnitOfWork uow) => _uow = uow;

        //gitfs crud
        public async Task<List<Gift>> GetGiftsAsync()
            => (await _uow.Gifts.FindAsync(g => g.IsActive))
                .OrderBy(g => g.RequiredPoints).ToList();

        public async Task<Gift?> GetGiftByIdAsync(int id)
            => await _uow.Gifts.GetByIdAsync(id);

        public async Task CreateGiftAsync(Gift gift)
        {
            await _uow.Gifts.AddAsync(gift);
            await _uow.SaveChangesAsync();
        }

        public async Task UpdateGiftAsync(Gift gift)
        {
            _uow.Gifts.Update(gift);
            await _uow.SaveChangesAsync();
        }

        public async Task DeleteGiftAsync(int id)
        {
            var gift = await _uow.Gifts.GetByIdAsync(id);
            if (gift == null) return;
            gift.IsActive = false;
            _uow.Gifts.Update(gift);
            await _uow.SaveChangesAsync();
        }

        //talepler
        public async Task<List<GiftRequest>> GetPendingRequestsAsync()
            => await _uow.GiftRequests.Query()
                .Include(r => r.Gift)
                .Include(r => r.Visitor)
                .Where(r => r.Status == GiftRequestStatus.Pending)
                .OrderBy(r => r.CreatedDate)
                .ToListAsync();

        public async Task<(bool ok, string message)> ApproveRequestAsync(int requestId)
        {
            var request = await _uow.GiftRequests.Query()
                .Include(r => r.Gift)
                .FirstOrDefaultAsync(r => r.Id == requestId);

            if (request == null || request.Status != GiftRequestStatus.Pending)
                return (false, "Talep bulunamadı veya zaten işlenmiş.");

            if (request.Gift.Stock <= 0)
                return (false, "Hediye stoğu tükenmiş.");

            var balance = await GetBalanceAsync(request.VisitorId);
            if (balance < request.Gift.RequiredPoints)
                return (false, "Ziyaretçinin puanı yetersiz.");

            //üç değişiklik ama tek transaction
            request.Status = GiftRequestStatus.Approved;//talep onaylandı
            request.ProcessedDate = DateTime.Now;
            request.Gift.Stock -= 1;   //stok düştü

            await _uow.LoyaltyTransactions.AddAsync(new LoyaltyTransaction
            { //puan harcandı
                VisitorId = request.VisitorId,
                Points = -request.Gift.RequiredPoints,//eksi puan
                Source = LoyaltySource.GiftSpending,
                Description = $"{request.Gift.Name} hediyesi alındı"
            });

            await _uow.SaveChangesAsync(); //ya üçü birden ya hiçbiri
            return (true, "Talep onaylandı.");
        }

        public async Task RejectRequestAsync(int requestId)
        {
            var request = await _uow.GiftRequests.GetByIdAsync(requestId);
            if (request == null || request.Status != GiftRequestStatus.Pending) return;

            request.Status = GiftRequestStatus.Rejected;
            request.ProcessedDate = DateTime.Now;
            _uow.GiftRequests.Update(request);
            await _uow.SaveChangesAsync();
        }

        
        public async Task<int> GetBalanceAsync(string visitorId)
            => (await _uow.LoyaltyTransactions.FindAsync(t => t.VisitorId == visitorId))
                .Sum(t => t.Points);


        public async Task<List<LoyaltyTransaction>> GetTransactionsAsync(string visitorId)
    => (await _uow.LoyaltyTransactions.FindAsync(t => t.VisitorId == visitorId))
        .OrderByDescending(t => t.CreatedDate).ToList();

        public async Task<(bool ok, string message)> RequestGiftAsync(string visitorId, int giftId)
        {
            var gift = await _uow.Gifts.GetByIdAsync(giftId);
            if (gift == null || !gift.IsActive)
                return (false, "Hediye bulunamadı.");

            if (gift.Stock <= 0)
                return (false, "Bu hediyenin stoğu tükenmiş.");

            var balance = await GetBalanceAsync(visitorId);
            if (balance < gift.RequiredPoints)
                return (false, $"Yetersiz puan. Gereken: {gift.RequiredPoints}, mevcut: {balance}.");

            // Aynı hediyeye bekleyen talebi varsa ikinciyi engelle
            var existing = await _uow.GiftRequests.FirstOrDefaultAsync(
                r => r.VisitorId == visitorId && r.GiftId == giftId
                  && r.Status == GiftRequestStatus.Pending);
            if (existing != null)
                return (false, "Bu hediye için zaten bekleyen bir talebiniz var.");

            await _uow.GiftRequests.AddAsync(new GiftRequest
            {
                VisitorId = visitorId,
                GiftId = giftId,
                Status = GiftRequestStatus.Pending
            });
            await _uow.SaveChangesAsync();

            return (true, $"{gift.Name} talebiniz alındı. Admin onayı bekleniyor.");
        }

        public async Task<List<GiftRequest>> GetMyRequestsAsync(string visitorId)
            => await _uow.GiftRequests.Query()
                .Include(r => r.Gift)
                .Where(r => r.VisitorId == visitorId)
                .OrderByDescending(r => r.CreatedDate)
                .ToListAsync();
    }
}