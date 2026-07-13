using LibraryProject.Model.Entities;

namespace LibraryProject.Business.Interfaces
{
    public interface ILoyaltyService
    {
        //hediye crud
        Task<List<Gift>> GetGiftsAsync();
        Task<Gift?> GetGiftByIdAsync(int id);
        Task CreateGiftAsync(Gift gift);
        Task UpdateGiftAsync(Gift gift);
        Task DeleteGiftAsync(int id);

        //talepler
        Task<List<GiftRequest>> GetPendingRequestsAsync();
        Task<(bool ok, string message)> ApproveRequestAsync(int requestId);
        Task RejectRequestAsync(int requestId);
        Task<int> GetBalanceAsync(string visitorId);
        Task<List<LoyaltyTransaction>> GetTransactionsAsync(string visitorId);
        Task<(bool ok, string message)> RequestGiftAsync(string visitorId, int giftId);
        Task<List<GiftRequest>> GetMyRequestsAsync(string visitorId);

    }
}