using FCG.Payments.Infrastructure.Persistence;
using FCG.Payments.Domain.EventSourcing;
using Microsoft.EntityFrameworkCore;

namespace FCG.Payments.Infrastructure.EventSourcing;

public class EventStoreRepository : IEventStoreRepository
{
    private readonly PaymentContext _context;

    public EventStoreRepository(PaymentContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<StoredEvent>> GetEventsByAggregateId(Guid aggregateId)
    {
        return await _context.StoredEvents
            .AsNoTracking()
            .Where(e => e.AggregateId == aggregateId)
            .OrderBy(e => e.OccurredOn)
            .ToListAsync();
    }

    public async Task<IEnumerable<StoredEvent>> GetEventsByType(string eventType)
    {
        return await _context.StoredEvents
            .AsNoTracking()
            .Where(e => e.EventType == eventType)
            .OrderBy(e => e.OccurredOn)
            .ToListAsync();
    }
}
