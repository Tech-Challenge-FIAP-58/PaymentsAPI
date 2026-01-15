using FCG.Payments.Models;
using FCG.Payments.Models.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FCG.Payments.Data.Repositories;

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

    public async Task<Payment> GetPaymentByOrderId(int orderId)
    {
        return await _context.Payments.AsNoTracking()
                        .FirstOrDefaultAsync(p => p.OrderId == orderId);
    }

    public async Task<IEnumerable<Transaction>> GetTransactionsByOrderId(int orderId)
    {
        return await _context.Transactions.AsNoTracking()
            .Where(t => t.Payment.OrderId == orderId).ToListAsync();
    }
}
