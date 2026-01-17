using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using FCG.Payments.Models;

namespace FCG.Payments.Data.Mappings
{
    public class PaymentMapping : IEntityTypeConfiguration<Payment>
    {
        public void Configure(EntityTypeBuilder<Payment> builder)
        {
            builder.HasKey(c => c.Id);

            builder.Property(c => c.Id)
                .ValueGeneratedOnAdd();

            builder.Property(c => c.Status)
                .IsRequired()
                .HasConversion<string>();

            builder.Property(c => c.PaymentMethod)
                .IsRequired()
                .HasConversion<string>();

            builder.HasIndex(p => p.OrderId)
                   .HasDatabaseName("IX_Payment_OrderId");

            // Cria um indice único no OrderId, mas aplica um filtro SQL (WHERE)
            builder.HasIndex(p => p.OrderId)
                .IsUnique()
                .HasFilter("[Status] = 'Approved'");

            builder.Ignore(c => c.CreditCard);

            // 1 : N => Pagamento : Transacao
            builder.HasMany(c => c.Transactions)
                .WithOne(c => c.Payment)
                .HasForeignKey(c => c.PaymentId);

            builder.ToTable("Payments");
        }
    }
}