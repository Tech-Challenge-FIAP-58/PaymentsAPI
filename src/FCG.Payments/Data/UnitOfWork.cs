using Microsoft.EntityFrameworkCore.Storage;
using FCG.Core.Data.Interfaces;

namespace FCG.Payments.Data;

public class UnitOfWork : IUnitOfWork
{
    private readonly PaymentContext _context;
    private IDbContextTransaction? _transaction;

    public UnitOfWork(PaymentContext context)
    {
        _context = context;
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
            throw new InvalidOperationException("Já existe uma transação ativa.");

        _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }


    public async Task<bool> CommitAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction == null)
            throw new InvalidOperationException("Nenhuma transação ativa para commitar.");

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
            await _transaction.CommitAsync(cancellationToken);
            return true;
        }
        catch
        {
            await RollBackTransactionAsync(cancellationToken);
            throw;
        }
    }

    public async Task RollBackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction == null)
            return;

        await _transaction.RollbackAsync(cancellationToken);
    }
}
