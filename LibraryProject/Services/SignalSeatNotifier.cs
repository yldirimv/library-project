using LibraryProject.Business.Interfaces;
using LibraryProject.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace LibraryProject.Services
{
    public class SignalRSeatNotifier : ISeatNotifier
    {
        private readonly IHubContext<NotificationHub> _hub;

        public SignalRSeatNotifier(IHubContext<NotificationHub> hub) => _hub = hub;

        public Task NotifySeatChangedAsync()
            => _hub.Clients.Group("employees").SendAsync("SeatChanged");
    }
}