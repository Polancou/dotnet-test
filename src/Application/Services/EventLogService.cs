using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services;

public class EventLogService(IEventLogRepository eventLogRepository, IUnitOfWork unitOfWork, IEventNotifier eventNotifier) : IEventLogService
{
    public async Task<IEnumerable<EventLog>> GetLogsAsync(int userId, UserRole role)
    {
        if (role == UserRole.Admin)
        {
            return await eventLogRepository.GetAllAsync();
        }
        
        if (role != UserRole.Admin)
        {
             return Enumerable.Empty<EventLog>();
        }

        return await eventLogRepository.GetAllAsync();
    }

    public async Task LogEventAsync(string eventType, string details, int? userId = null)
    {
        var log = new EventLog(eventType, details, userId);
        await eventLogRepository.AddAsync(log);
        await unitOfWork.SaveChangesAsync();

        await eventNotifier.NotifyAsync("ReceiveLog", new 
        { 
            Id = log.Id, 
            EventType = log.EventType, 
            Details = log.Description, 
            Timestamp = log.CreationDate 
        });
    }
}
