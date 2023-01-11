using System.Collections.Generic;

namespace Tanka.GraphQL.Experimental;

public class SubscriberBuilder
{
    private readonly List<SubscriberMiddleware> _middlewares = new();

    private Subscriber _root;

    public SubscriberBuilder(Subscriber root)
    {
        Run(root);
    }

    public SubscriberBuilder()
    {
    }

    /// <summary>
    ///     Add middlware to be run before the root of the chain
    /// </summary>
    /// <param name="middleware"></param>
    /// <returns></returns>
    public SubscriberBuilder Use(SubscriberMiddleware middleware)
    {
        _middlewares.Insert(0, middleware);
        return this;
    }

    /// <summary>
    ///     Set root subscriber to be run at the end of the subscriber chain
    /// </summary>
    /// <param name="resolver"></param>
    /// <returns></returns>
    public SubscriberBuilder Run(Subscriber subscriber)
    {
        _root = subscriber;
        return this;
    }

    public Subscriber Build()
    {
        var subscriber = _root;
        foreach (var middleware in _middlewares)
        {
            var subscriber1 = subscriber;
            subscriber = (context, unsubscribe) => middleware(context, unsubscribe, subscriber1);
        }

        return subscriber;
    }
}