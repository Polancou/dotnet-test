using Application.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Api.Hubs;

namespace Api.Services;

public class SignalREventNotifier(IHubContext<EventLogHub> hubContext) : IEventNotifier
{
    public async Task NotifyAsync(string eventType, object data)
    {
        await hubContext.Clients.All.SendAsync(eventType, data);
    }
}
