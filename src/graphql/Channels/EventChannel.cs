﻿using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tanka.GraphQL.ValueResolution;

namespace Tanka.GraphQL.Channels
{
    public class EventChannel<T>
    {
        private readonly ConcurrentDictionary<CancellationToken, ISubscriberResult> 
            _subscriptions = new ConcurrentDictionary<CancellationToken, ISubscriberResult>();

        protected IEnumerable<KeyValuePair<CancellationToken, ISubscriberResult>> Subscriptions => _subscriptions;

        public ISubscriberResult Subscribe(CancellationToken unsubscribe)
        {
            var subscription = new SubscriberResult();
            _subscriptions[unsubscribe] = subscription;

            unsubscribe.Register(() =>
            {
                if (_subscriptions.TryRemove(unsubscribe, out var sub))
                {
                    OnUnsubscribing(sub);
                    sub.TryComplete();
                }
            });

            OnSubscribed(subscription);
            return subscription;
        }

        public virtual void OnSubscribed(ISubscriberResult subscription)
        {
        }

        public virtual void OnUnsubscribing(ISubscriberResult subscription)
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