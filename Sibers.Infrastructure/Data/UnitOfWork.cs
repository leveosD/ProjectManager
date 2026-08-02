using Microsoft.EntityFrameworkCore.Storage;
using Sibers.Core.Interfaces;

namespace Sibers.Infrastructure.Data;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _dbContext;
    private IDbContextTransaction? _currentTransaction;

    public UnitOfWork(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task BeginTransactionAsync()
    {
        // Если транзакция уже открыта, не открываем новую
        if (_currentTransaction != null) return;

        _currentTransaction = await _dbContext.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        try
        {
            // Сохраняем все накопленные изменения (если они есть)
            await _dbContext.SaveChangesAsync();
            
            // Если транзакция была открыта, коммитим её
            if (_currentTransaction != null)
            {
                await _currentTransaction.CommitAsync();
            }
        }
        finally
        {
            if (_currentTransaction != null)
            {
                await _currentTransaction.DisposeAsync();
                _currentTransaction = null;
            }
        }
    }

    public async Task RollbackTransactionAsync()
    {
        try
        {
            if (_currentTransaction != null)
            {
                await _currentTransaction.RollbackAsync();
            }
        }
        finally
        {
            if (_currentTransaction != null)
            {
                await _currentTransaction.DisposeAsync();
                _currentTransaction = null;
            }
        }
    }
    
    public async Task ExecuteInTransactionAsync(Func<Task> action)
    {
        await BeginTransactionAsync();
        try
        {
            await action();
            await CommitTransactionAsync();
        }
        catch
        {
            await RollbackTransactionAsync();
            throw;
        }
    }

    public async Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> action)
    {
        await BeginTransactionAsync();
        try
        {
            var result = await action();
            await CommitTransactionAsync(); 
            return result;
        }
        catch
        {
            await RollbackTransactionAsync();
            throw; 
        }
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        _currentTransaction?.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        await _dbContext.DisposeAsync();
        if (_currentTransaction != null)
        {
            await _currentTransaction.DisposeAsync();
        }
    }
}