using LibraryProject.Model.Dtos;
using LibraryProject.Model.Entities;

namespace LibraryProject.Business.Interfaces
{
    public interface IReservationService
    {
        Task<List<Reservation>> GetMyReservationsAsync(string visitorId);
        Task<List<Seat>> GetSeatsAsync(int floor);
        Task<List<int>> GetOccupiedSeatIdsAsync(DateTime start, DateTime end);
        Task<(bool ok, string message)> CreateAsync(string visitorId, int seatId,
                                                    DateTime start, DateTime end);
        Task<(bool ok, string message)> CancelAsync(int reservationId, string visitorId);
        Task<Dictionary<int, SeatStateDto>> GetCurrentSeatStatesAsync();
        Task<List<Reservation>> GetTodaysReservationsAsync();
    }
}