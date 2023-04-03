namespace Tanka.GraphQL.ValueResolution;

public class SubscriberBuilder
{
    private readonly List<Func<Subscriber, Subscriber>> _components = new();

    public SubscriberBuilder Use(Func<Subscriber, Subscriber> middleware)
    {
        _components.Add(middleware);
        return this;
    }

    public SubscriberBuilder Use(Delegate middleware)
    {
        var middlewareFunc = DelegateSubscriberMiddlewareFactory.Get(middleware);
        _components.Add(next => (context, unsubscribe) => middlewareFunc(context, next, unsubscribe));
        return this;
    }

    public SubscriberBuilder Run(Subscriber subscriber)
    {
        return Use(_ => subscriber);
    }

    public SubscriberBuilder Run(Delegate subscriber)
    {
        return Use(_ => DelegateSubscriberFactory.Get(subscriber));
    }

    public Subscriber Build()
    {
        Subscriber subscriber = (_, _) => ValueTask.CompletedTask;

        for (int c = _components.Count - 1; c >= 0; c--)
            subscriber = _components[c](subscriber);

        return subscriber;
    }
}