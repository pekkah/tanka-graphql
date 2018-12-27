using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace graphql.server.tests.host
{
    public class EventManager
    {
        private readonly ConcurrentDictionary<int, EventSubscription> _subscriptions =
            new ConcurrentDictionary<int, EventSubscription>();

        private int _lastId;

        public Task<ChannelReader<object>> SubscribeAsync(CancellationToken unsubscribe)
        {
            var newId = Interlocked.Increment(ref _lastId);
            var subscription = _subscriptions.GetOrAdd(
                newId,
                new EventSubscription(newId));

            unsubscribe.Register(() =>
            {
                if (_subscriptions.TryRemove(newId, out var sub))
                    sub.Unsubscribe();
            });

            return Task.FromResult(subscription.Reader);
        }

        public async Task HelloAllAsync(string message)
        {
            foreach (var (_, subscription) in _subscriptions)
                await subscription.HelloAsync(message);
        }

        public async Task HelloAsync(int id, string message)
        {
            if (_subscriptions.TryGetValue(id, out var subscription)) 
                await subscription.HelloAsync(message);
        }
    }
}