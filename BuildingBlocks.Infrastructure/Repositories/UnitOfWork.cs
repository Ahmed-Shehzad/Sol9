using BuildingBlocks.Contracts.Types;
using BuildingBlocks.Utilities.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BuildingBlocks.Infrastructure.Repositories;

public class UnitOfWork<TContext>(TContext context) : IUnitOfWork, IUnitOfWorkContext, IDisposable
    where TContext : DbContext
{
    private IDbContextTransaction? _transaction;
    public async Task CommitAsync(CancellationToken cancellationToken = default)
    { 
        await context.SaveChangesAsync(cancellationToken);
    }
    
    public async Task CommitAsync(Func<IUnitOfWorkContext, Task> operation, CancellationToken cancellationToken = default)
    {
        // Begin the transaction
        _transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            // Execute the operation with the UnitOfWork context
            await operation(this);

            // Commit the transaction at the end
            await _transaction.CommitAsync(cancellationToken);
        }
        catch(Exception ex)
        {
            // Rollback the transaction on error
            if (_transaction != null) await _transaction.RollbackAsync(cancellationToken);
            throw new DatabaseUpdateException(ex.Message, ex);
        }
        finally
        {
            // Clean up
            if (_transaction != null) await _transaction.DisposeAsync();
            _transaction = null;
        }
    }
    
    public async Task<T> CommitAsync<T>(Func<IUnitOfWorkContext, Task<T>> operation, CancellationToken cancellationToken = default)
    {
        // Begin the transaction
        _transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            // Execute the operation with the UnitOfWork context
            var result = await operation(this);

            // Commit the transaction at the end
            await _transaction.CommitAsync(cancellationToken);
            return result;
        }
        catch(Exception ex)
        {
            // Rollback the transaction on error
            if (_transaction != null) await _transaction.RollbackAsync(cancellationToken);
            throw new DatabaseUpdateException(ex.Message, ex);
        }
        finally
        {
            // Clean up
            if (_transaction != null) await _transaction.DisposeAsync();
            _transaction = null;
        }
    }
    
    public void Dispose()
    {
        _transaction?.Dispose();
        context.Dispose();
    }
}