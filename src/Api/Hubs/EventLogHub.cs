using Microsoft.AspNetCore.SignalR;

namespace Api.Hubs;

public class EventLogHub : Hub
{
    public async Task SendLog(string message)
    {
        await Clients.All.SendAsync("ReceiveLog", message);
    }
}
