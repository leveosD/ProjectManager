namespace Sibers.Core.Interfaces;

public interface IUnitOfWork : IAsyncDisposable, IDisposable
{
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
    Task ExecuteInTransactionAsync(Func<Task> action);
    Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> action);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}