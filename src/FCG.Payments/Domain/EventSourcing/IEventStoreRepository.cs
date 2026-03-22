namespace FCG.Payments.Domain.EventSourcing;

public interface IEventStoreRepository
{
    Task<IEnumerable<StoredEvent>> GetEventsByAggregateId(Guid aggregateId);
    Task<IEnumerable<StoredEvent>> GetEventsByType(string eventType);
}
