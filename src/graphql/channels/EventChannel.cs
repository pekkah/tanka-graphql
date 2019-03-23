using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using tanka.graphql.resolvers;

namespace tanka.graphql.channels
{
    public class EventChannel<T>
    {
        private readonly ConcurrentDictionary<CancellationToken, ISubscribeResult> 
            _subscriptions = new ConcurrentDictionary<CancellationToken, ISubscribeResult>();

        protected IEnumerable<KeyValuePair<CancellationToken, ISubscribeResult>> Subscriptions => _subscriptions;

        public ISubscribeResult Subscribe(CancellationToken unsubscribe)
        {
            var subscription = new SubscribeResult();
            _subscriptions[unsubscribe] = subscription;

            unsubscribe.Register(() =>
            {
                if (_subscriptions.TryRemove(unsubscribe, out var sub))
                {
                    OnUnsubscribing(subscription);
                    sub.TryComplete();
                }
            });

            OnSubscribed(subscription);
            return subscription;
        }

        public virtual void OnSubscribed(SubscribeResult subscription)
        {
        }

        public virtual void OnUnsubscribing(SubscribeResult subscription)
        {
        }

        public virtual async ValueTask WriteAsync(T item)
        {
            foreach (var subscription in _subscriptions)
            {
                await subscription.Value.WriteAsync(item, subscription.Key);
            }
        }
    }
}