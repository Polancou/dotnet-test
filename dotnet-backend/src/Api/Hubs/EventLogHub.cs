using Microsoft.AspNetCore.SignalR;

namespace Api.Hubs;

/// <summary>
/// SignalR hub for broadcasting event log updates to connected clients.
/// </summary>
public class EventLogHub : Hub
{
    /// <summary>
    /// Receives a log message from a client or server, and broadcasts it to all connected clients.
    /// </summary>
    /// <param name="message">The log message to broadcast.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task SendLog(string message)
    {
        // Broadcast the message to all connected SignalR clients using the "ReceiveLog" method.
        await Clients.All.SendAsync("ReceiveLog", message);
    }
}
