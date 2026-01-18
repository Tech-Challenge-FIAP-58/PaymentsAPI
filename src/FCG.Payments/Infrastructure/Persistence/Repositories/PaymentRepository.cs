using FCG.Payments.Domain.Entities;
using FCG.Payments.Domain.Entities.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;

namespace FCG.Payments.Infrastructure.Persistence.Repositories;

[ExcludeFromCodeCoverage]
public class PaymentRepository : IPaymentRepository
{
    private readonly PaymentContext _context;

    public PaymentRepository(PaymentContext context)
    {
        _context = context;
    }

    public void AddPayment(Payment payment)
    {
        _context.Payments.Add(payment);
    }

    public void AddTransaction(Transaction transaction)
    {
        _context.Transactions.Add(transaction);
    }

    public async Task<IEnumerable<Payment>> GetPaymentByOrderId(Guid orderId)
    {
        return await _context.Payments.AsNoTracking()
                        .Include(p => p.Transactions)
                        .Where(p => p.OrderId == orderId).ToListAsync();
    }

    public async Task<IEnumerable<Transaction>> GetTransactionsByOrderId(Guid orderId)
    {
        return await _context.Transactions.AsNoTracking()
            .Where(t => t.Payment.OrderId == orderId).ToListAsync();
    }
}
