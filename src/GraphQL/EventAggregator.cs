using System.Collections.Concurrent;
using System.Threading.Channels;

namespace Tanka.GraphQL;

public class EventAggregator<T>
{
    private readonly ConcurrentDictionary<Channel<T>, byte> _channels = new();

    public IAsyncEnumerable<T> Subscribe(CancellationToken cancellationToken = default)
    {
        var channel = Channel.CreateUnbounded<T>(new UnboundedChannelOptions()
        {
            SingleReader = true,
            SingleWriter = false
        });
        _channels.TryAdd(channel, 0);

        cancellationToken.Register(Remove);
        
        return new AsyncEnumerable(channel.Reader, Remove);

        void Remove()
        {
            _channels.TryRemove(channel, out _);
            channel.Writer.TryComplete();
        }
    }

    public int SubscriberCount => _channels.Count;

    public async ValueTask Publish(T item, CancellationToken cancellationToken = default)
    {
        foreach (var (channel, _) in _channels)
        {
            await channel.Writer.WriteAsync(item, cancellationToken);
        }
    }

    private class AsyncEnumerable(ChannelReader<T> reader, Action onDisposed)
        : IAsyncEnumerable<T>
    {
        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            return new AsyncEnumerator(reader, onDisposed, cancellationToken);
        }
    }

    private class AsyncEnumerator : IAsyncEnumerator<T>
    {
        private bool _disposed;
        private readonly ChannelReader<T> _reader;
        private readonly Action _onDisposed;

        public AsyncEnumerator(ChannelReader<T> reader, Action onDisposed, CancellationToken cancellationToken)
        {
            _reader = reader;
            _onDisposed = onDisposed;
            cancellationToken.Register(onDisposed);
        }

        public T Current { get; private set; } = default!;

        public ValueTask DisposeAsync()
        {
            if (_disposed)
                return default;

            _onDisposed();
            _disposed = true;
            return default;
        }

        public async ValueTask<bool> MoveNextAsync()
        {
            try
            {
                Current = await _reader.ReadAsync();
                return true;
            }
            catch (ChannelClosedException)
            {
                return false;
            }
            catch (OperationCanceledException)
            {
                return false;
            }
        }
    }

    public async Task WaitForSubscribers(TimeSpan timeout)
    {
        var tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        _ = Task.Factory.StartNew(async () =>
        {
            using var cts = new CancellationTokenSource(timeout);
            while (SubscriberCount == 0)
            {
                await Task.Delay(100, cts.Token);
            }
            
            tcs.SetResult();
        }, TaskCreationOptions.RunContinuationsAsynchronously);

        await tcs.Task;
    }

    public async Task WaitForAtLeastSubscribers(TimeSpan timeout, int atLeast)
    {
        var tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        _ = Task.Factory.StartNew(async () =>
        {
            using var cts = new CancellationTokenSource(timeout);
            while (SubscriberCount < atLeast)
            {
                await Task.Delay(100, cts.Token);
            }
            
            tcs.SetResult();
        }, TaskCreationOptions.RunContinuationsAsynchronously);

        await tcs.Task;
    }

    public async Task WaitForNoSubscribers(TimeSpan timeout)
    {
        var tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        _ = Task.Factory.StartNew(async () =>
        {
            using var cts = new CancellationTokenSource(timeout);
            while (SubscriberCount > 0)
            {
                await Task.Delay(100, cts.Token);
            }
            
            tcs.SetResult();
        }, TaskCreationOptions.RunContinuationsAsynchronously);

        await tcs.Task;
    }
}