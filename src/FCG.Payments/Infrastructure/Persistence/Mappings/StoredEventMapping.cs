using FCG.Payments.Domain.EventSourcing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FCG.Payments.Infrastructure.Persistence.Mappings;

public class StoredEventMapping : IEntityTypeConfiguration<StoredEvent>
{
    public void Configure(EntityTypeBuilder<StoredEvent> builder)
    {
        builder.ToTable("StoredEvents");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.AggregateId).IsRequired();
        builder.Property(e => e.AggregateType).IsRequired().HasMaxLength(100);
        builder.Property(e => e.EventType).IsRequired().HasMaxLength(100);
        builder.Property(e => e.Payload).IsRequired().HasColumnType("nvarchar(max)");
        builder.Property(e => e.OccurredOn).IsRequired();
    }
}
