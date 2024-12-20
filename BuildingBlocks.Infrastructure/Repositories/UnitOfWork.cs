using BuildingBlocks.Contracts.Types;
using BuildingBlocks.Utilities.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BuildingBlocks.Infrastructure.Repositories;

public class UnitOfWork<TContext>(TContext context) : IUnitOfWork, IDisposable
    where TContext : DbContext
{
    private IDbContextTransaction? _transaction;
    private bool _disposed = false;
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
        catch (Exception ex)
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
        catch (Exception ex)
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

    /// <summary>
    /// Disposes the transaction and the context.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes the transaction and the context.
    /// </summary>
    /// <param name="disposing">Indicates whether the method is called from Dispose or the finalizer.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;
        if (disposing)
        {
            _transaction?.Dispose();
            context?.Dispose();
        }
        _disposed = true;
    }

    ~UnitOfWork()
    {
        Dispose(false);
    }
}