using Domain.Common;

namespace Domain.Entities;

/// <summary>
/// Represents an event log entry in the system.
/// </summary>
public class EventLog : BaseEntity
{
    /// <summary>
    /// Gets the type of the event.
    /// </summary>
    public string EventType { get; private set; }

    /// <summary>
    /// Gets the description of the event.
    /// </summary>
    public string Description { get; private set; }

    /// <summary>
    /// Gets the ID of the user associated with the event. Nullable if it's a system event.
    /// </summary>
    public int? UserId { get; private set; } // Nullable if system event

    /// <summary>
    /// Gets the user associated with the event.
    /// </summary>
    public User? User { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="EventLog"/> class.
    /// This constructor is primarily for EF Core.
    /// </summary>
    private EventLog() { } // EF Core

    /// <summary>
    /// Initializes a new instance of the <see cref="EventLog"/> class with specified event details.
    /// </summary>
    /// <param name="eventType">The type of the event.</param>
    /// <param name="description">A detailed description of the event.</param>
    /// <param name="userId">The optional ID of the user who performed the event. Null if it's a system event.</param>
    /// <exception cref="ArgumentException">Thrown if <paramref name="eventType"/> or <paramref name="description"/> is null or whitespace.</exception>
    public EventLog(string eventType, string description, int? userId = null) : base(true)
    {
        if (string.IsNullOrWhiteSpace(eventType)) throw new ArgumentException("Event type cannot be empty.", nameof(eventType));
        if (string.IsNullOrWhiteSpace(description)) throw new ArgumentException("Description cannot be empty.", nameof(description));

        EventType = eventType;
        Description = description;
        UserId = userId;
    }
}
