using Application.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Api.Hubs;

namespace Api.Services;

/// <summary>
/// Sends real-time event notifications to all connected SignalR clients.
/// Implements IEventNotifier for application-level event broadcasting.
/// </summary>
public class SignalREventNotifier : IEventNotifier
{
    private readonly IHubContext<EventLogHub> _hubContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="SignalREventNotifier"/> class.
    /// </summary>
    /// <param name="hubContext">The SignalR hub context for the EventLogHub.</param>
    public SignalREventNotifier(IHubContext<EventLogHub> hubContext)
    {
        _hubContext = hubContext;
    }

    /// <summary>
    /// Notifies all connected clients of a specified event type and associated data.
    /// </summary>
    /// <param name="eventType">The name of the event to be sent (SignalR method name).</param>
    /// <param name="data">The payload data to broadcast with the event.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task NotifyAsync(string eventType, object data)
    {
        // Broadcast the event to all connected SignalR clients.
        await _hubContext.Clients.All.SendAsync(eventType, data);
    }
}
