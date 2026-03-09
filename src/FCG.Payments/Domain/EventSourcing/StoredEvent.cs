namespace FCG.Payments.Domain.EventSourcing;

public class StoredEvent
{
    public Guid Id { get; private set; }
    public Guid AggregateId { get; private set; }
    public string AggregateType { get; private set; }
    public string EventType { get; private set; }
    public string Payload { get; private set; }
    public DateTime OccurredOn { get; private set; }

    protected StoredEvent() { }

    public StoredEvent(Guid aggregateId, string aggregateType, string eventType, string payload)
    {
        Id = Guid.NewGuid();
        AggregateId = aggregateId;
        AggregateType = aggregateType;
        EventType = eventType;
        Payload = payload;
        OccurredOn = DateTime.UtcNow;
    }
}
