using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace LibraryProject.Hubs
{
    [Authorize]
    public class NotificationHub : Hub
    {
        // Personel sayfası yüklenince kendini "employees" grubuna yazdırır
        public async Task JoinEmployeeGroup()
        {
            if (Context.User != null &&
        (Context.User.IsInRole("Employee") || Context.User.IsInRole("Admin")))
                await Groups.AddToGroupAsync(Context.ConnectionId, "employees");
        }
    }
}