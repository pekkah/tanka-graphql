using System.Collections.Immutable;
using System.Threading.Channels;

namespace Tanka.GraphQL.Subscriptions;

public class BroadcastChannel<T> : IAsyncDisposable
{
    private readonly ChannelReader<T> _source;

    private readonly object _startBroadcastingLock = new();
    private Task? _broadcastTask;
    private CancellationTokenSource _cancelBroadcast = new();


    private ImmutableArray<Channel<T>> _subscriptions = ImmutableArray<Channel<T>>.Empty;

    public BroadcastChannel(ChannelReader<T> source)
    {
        _source = source;
    }

    public Task Completion => _broadcastTask ?? Task.CompletedTask;

    public async ValueTask DisposeAsync()
    {
        _cancelBroadcast.Cancel();
        _cancelBroadcast.Dispose();
        await Completion;
    }

    public IAsyncEnumerable<T> Subscribe(CancellationToken cancellationToken)
    {
        var subscription = Channel.CreateUnbounded<T>();
        ImmutableInterlocked.Update(ref _subscriptions, s => s.Add(subscription));

        cancellationToken.Register(Unsubscribe);

        if (_broadcastTask is null)
            lock (_startBroadcastingLock)
            {
                _broadcastTask ??= StartBroadcasting();
            }

        return subscription.Reader.ReadAllAsync(cancellationToken);

        void Unsubscribe()
        {
            ImmutableInterlocked.Update(ref _subscriptions, s => s.Remove(subscription));

            subscription.Writer.Complete();
        }
    }

    private async Task StartBroadcasting()
    {
        var cancellationToken = _cancelBroadcast.Token;

        try
        {
            while (await _source.WaitToReadAsync(cancellationToken))
            {
                cancellationToken.ThrowIfCancellationRequested();

                var item = await _source.ReadAsync(cancellationToken);

                var tasks = new List<Task>(_subscriptions.Length);
                foreach (var subscription in _subscriptions)
                {
                    var task = subscription.Writer.WriteAsync(item, cancellationToken).AsTask();
                    tasks.Add(task);
                }

                await Task.WhenAll(tasks);
            }
        }
        catch (OperationCanceledException)
        {
            //noop
        }

        foreach (var subscription in _subscriptions) subscription.Writer.Complete();
    }
}