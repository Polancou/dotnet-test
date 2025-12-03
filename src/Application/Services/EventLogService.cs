using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services;

public class EventLogService(IEventLogRepository eventLogRepository) : IEventLogService
{
    public async Task<IEnumerable<EventLog>> GetLogsAsync(int userId, UserRole role)
    {
        if (role == UserRole.Admin)
        {
            return await eventLogRepository.GetAllAsync();
        }
        
        // For now, regular users only see their own logs (if we had a method for that)
        // Or maybe they don't see logs at all?
        // Let's assume for now we filter by user ID if not admin, 
        // but GenericRepository might not have a specific filter method exposed easily without specification pattern.
        // Let's check IEventLogRepository.
        
        // Actually, let's just return all for now or filter in memory (inefficient but works for small scale)
        // Better: Add GetByUserIdAsync to IEventLogRepository?
        // Let's stick to simple: Admin sees all. User sees none? Or their own?
        // Requirement says "Event Log: View system events and history."
        // Let's assume Admin sees all.
        
        if (role != UserRole.Admin)
        {
             return Enumerable.Empty<EventLog>();
        }

        return await eventLogRepository.GetAllAsync();
    }
}
