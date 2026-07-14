using System.Threading.Channels;

namespace CleanApi.Infrastructure.Jobs;

/// <summary>
/// An in-process work queue backed by <see cref="Channel{T}"/> — the lightweight, dependency-free
/// alternative to Hangfire for fire-and-forget work that need not survive a restart.
/// </summary>
public interface IBackgroundTaskQueue
{
    ValueTask EnqueueAsync(Func<IServiceProvider, CancellationToken, ValueTask> workItem);

    ValueTask<Func<IServiceProvider, CancellationToken, ValueTask>> DequeueAsync(CancellationToken cancellationToken);
}

public sealed class BackgroundTaskQueue : IBackgroundTaskQueue
{
    private readonly Channel<Func<IServiceProvider, CancellationToken, ValueTask>> _queue =
        Channel.CreateBounded<Func<IServiceProvider, CancellationToken, ValueTask>>(
            new BoundedChannelOptions(capacity: 100) { FullMode = BoundedChannelFullMode.Wait });

    public async ValueTask EnqueueAsync(Func<IServiceProvider, CancellationToken, ValueTask> workItem)
    {
        ArgumentNullException.ThrowIfNull(workItem);
        await _queue.Writer.WriteAsync(workItem);
    }

    public async ValueTask<Func<IServiceProvider, CancellationToken, ValueTask>> DequeueAsync(CancellationToken cancellationToken) =>
        await _queue.Reader.ReadAsync(cancellationToken);
}
