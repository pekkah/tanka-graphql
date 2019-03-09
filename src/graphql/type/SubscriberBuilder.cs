using System.Collections.Generic;
using tanka.graphql.resolvers;

namespace tanka.graphql.type
{
    public class SubscriberBuilder
    {
        private readonly List<SubscriberMiddleware> _middlewares
            = new List<SubscriberMiddleware>();

        public SubscriberBuilder Use(SubscriberMiddleware middleware)
        {
            _middlewares.Insert(0, middleware);
            return this;
        }

        public SubscriberBuilder Use(Subscriber subscriber)
        {
            _middlewares.Insert(0, (context, unsubscribe, next) => subscriber(context, unsubscribe));
            return this;
        }

        public Subscriber Build()
        {
            Subscriber subscriber = null;
            foreach (var middleware in _middlewares)
            {
                var subscriber1 = subscriber;
                subscriber = (context, unsubscribe) => middleware(context, unsubscribe, subscriber1);
            }

            return subscriber;
        }
    }
}