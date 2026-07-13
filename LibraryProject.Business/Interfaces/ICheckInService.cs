using LibraryProject.Model.Enums;

namespace LibraryProject.Business.Interfaces
{
    public interface ICheckInService
    {
        Task<(bool ok, string message)> ProcessAsync(
            string visitorId, string qrToken, ReservationEventType action);
    }
}