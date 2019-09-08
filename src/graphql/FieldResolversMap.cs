using System;
using System.Collections;
using System.Collections.Generic;
using tanka.graphql.resolvers;
using tanka.graphql.type;

namespace tanka.graphql
{
    public class FieldResolversMap : IEnumerable<Resolver>, IEnumerable<Subscriber>
    {
        private readonly Dictionary<string, Resolver> _resolvers = new Dictionary<string, Resolver>();
        private readonly Dictionary<string, Subscriber> _subscribers = new Dictionary<string, Subscriber>();

        IEnumerator<Resolver> IEnumerable<Resolver>.GetEnumerator()
        {
            return _resolvers.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            var list = new List<object>();
            list.AddRange(_resolvers.Values);
            list.AddRange(_subscribers.Values);
            return list.GetEnumerator();
        }

        IEnumerator<Subscriber> IEnumerable<Subscriber>.GetEnumerator()
        {
            return _subscribers.Values.GetEnumerator();
        }

        public void Add(string key, Resolver resolver)
        {
            _resolvers.Add(key, resolver);
        }

        public void Add(string key, Subscriber subscriber, Resolver resolver)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (subscriber == null) throw new ArgumentNullException(nameof(subscriber));
            if (resolver == null) throw new ArgumentNullException(nameof(resolver));

            _subscribers.Add(key, subscriber);
            _resolvers.Add(key, resolver);
        }

        public Resolver GetResolver(string key)
        {
            if (!_resolvers.ContainsKey(key))
                return null;

            return _resolvers[key];
        }

        public Subscriber GetSubscriber(string key)
        {
            if (!_subscribers.ContainsKey(key))
                return null;

            return _subscribers[key];
        }
    }
}