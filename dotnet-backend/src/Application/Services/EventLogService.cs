using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services;

/// <summary>
/// Service responsible for handling logging of application events and exposing event log retrieval.
/// Supports event notification and storage.
/// </summary>
public class EventLogService(
    IEventLogRepository eventLogRepository,
    IUnitOfWork unitOfWork,
    IEventNotifier eventNotifier
) : IEventLogService
{
    /// <summary>
    /// Retrieves event logs for the specified user, based on their role.
    /// Admin users are allowed to view all event logs.
    /// Non-admin users can only view their own event logs (filtered by userId).
    /// </summary>
    /// <param name="userId">The ID of the user requesting the logs (used for filtering non-admin users).</param>
    /// <param name="role">The role of the user requesting the logs.</param>
    /// <returns>An enumerable of <see cref="EventLog"/>s available to the requesting user.</returns>
    public async Task<IEnumerable<EventLog>> GetLogsAsync(int userId, UserRole role)
    {
        // If the user is an Admin, return all event logs.
        if (role == UserRole.Admin)
        {
            return await eventLogRepository.GetAllAsync();
        }

        // Non-admin users can only see their own logs (filtered by userId).
        return await eventLogRepository.FindAsync(log => log.UserId == userId);
    }

    /// <summary>
    /// Logs a new event in the application, persists it, and notifies listeners in real-time.
    /// </summary>
    /// <param name="eventType">The type/category of the event (e.g., "User Interaction").</param>
    /// <param name="details">Detailed description of the event.</param>
    /// <param name="userId">Optional ID of the user associated with the event.</param>
    public async Task LogEventAsync(string eventType, string details, int? userId = null)
    {
        // Create new event log object with details.
        var log = new EventLog(eventType, details, userId);

        // Add the new log entry to the repository (not yet persisted).
        await eventLogRepository.AddAsync(log);

        // Commit the change via unit of work to persist it in the database.
        await unitOfWork.SaveChangesAsync();

        // Notify any connected clients or listeners that a new event log was created.
        await eventNotifier.NotifyAsync("ReceiveLog", new
        {
            Id = log.Id,
            EventType = log.EventType,
            Details = log.Description,
            Timestamp = log.CreationDate
        });
    }
}
