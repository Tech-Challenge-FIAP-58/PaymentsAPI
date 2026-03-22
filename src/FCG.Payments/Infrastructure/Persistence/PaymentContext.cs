using FCG.Payments.Application.Mediator;
using FCG.Payments.Domain;
using FCG.Payments.Domain.Entities;
using FCG.Payments.Domain.Entities.Mediatr;
using FCG.Payments.Domain.EventSourcing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Text.Json;

namespace FCG.Payments.Infrastructure.Persistence
{
    public class PaymentContext : DbContext
    {
        private readonly IMediatorHandler _mediatorHandler;

        public PaymentContext(DbContextOptions<PaymentContext> options, 
            IMediatorHandler mediatorHandler)
            : base(options)
        {
            ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            ChangeTracker.AutoDetectChangesEnabled = false;
            _mediatorHandler = mediatorHandler;
        }

        public DbSet<Payment> Payments { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<StoredEvent> StoredEvents { get; set; }

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            // Define que TODA string, por padrão, será varchar(100)
            configurationBuilder.Properties<string>()
                .HaveColumnType("varchar(100)");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Ignore<ValidationResult>();
            modelBuilder.Ignore<Event>();

            foreach (var relationship in modelBuilder.Model.GetEntityTypes()
                .SelectMany(e => e.GetForeignKeys())) relationship.DeleteBehavior = DeleteBehavior.ClientSetNull;

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(PaymentContext).Assembly);
        }

        public override async Task<int> SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            foreach (var entry in ChangeTracker.Entries().Where(entry => entry.Entity.GetType().GetProperty("CreationTime") != null))
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Property("CreatedAt").CurrentValue = DateTime.Now;
                }

                if (entry.State == EntityState.Modified)
                {
                    entry.Property("CreatedAt").IsModified = false;
                    entry.Property("UpdatedAt").CurrentValue = DateTime.Now;
                }

                if (entry.State == EntityState.Modified && (bool)entry.Property("IsDeleted").CurrentValue)
                {
                    foreach (var property in entry.Properties)
                    {
                        property.IsModified = false;
                    }

                    entry.Property("IsDeleted").IsModified = true;
                    entry.Property("DeletedAt").IsModified = true;
                    entry.Property("DeletedAt").CurrentValue = DateTime.Now;
                }
            }

            var domainEntities = ChangeTracker.Entries<Entity>()
                .Where(x => x.Entity.Notificacoes != null && x.Entity.Notificacoes.Any())
                .ToList();

            var domainEvents = domainEntities
                .SelectMany(x => x.Entity.Notificacoes)
                .ToList();

            foreach (var entry in domainEntities)
                foreach (var domainEvent in entry.Entity.Notificacoes)
                    StoredEvents.Add(new StoredEvent(
                        entry.Entity.Id,
                        entry.Entity.GetType().Name,
                        domainEvent.GetType().Name,
                        JsonSerializer.Serialize((object)domainEvent)
                    ));

            domainEntities.ForEach(e => e.Entity.ClearEvents());

            var affectedRows = await base.SaveChangesAsync(cancellationToken);

            if (domainEvents.Count > 0)
            {
                var tasks = domainEvents.Select(e => _mediatorHandler.PublishEvent(e));
                await Task.WhenAll(tasks);
            }

            return affectedRows;
        }
    }
}
