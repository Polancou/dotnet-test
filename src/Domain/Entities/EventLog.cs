using Domain.Common;

namespace Domain.Entities;

public class EventLog : BaseEntity
{
    public string EventType { get; private set; }
    public string Description { get; private set; }
    public int? UserId { get; private set; } // Nullable if system event
    public User? User { get; private set; }

    private EventLog() { } // EF Core

    public EventLog(string eventType, string description, int? userId = null) : base(true)
    {
        if (string.IsNullOrWhiteSpace(eventType)) throw new ArgumentException("Event type cannot be empty.", nameof(eventType));
        if (string.IsNullOrWhiteSpace(description)) throw new ArgumentException("Description cannot be empty.", nameof(description));

        EventType = eventType;
        Description = description;
        UserId = userId;
    }
}
