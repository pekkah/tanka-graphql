using System.Collections;

namespace Tanka.GraphQL.ValueResolution;

public class FieldResolversMap : IEnumerable<Resolver>, IEnumerable<Subscriber>
{
    private readonly Dictionary<string, Resolver> _resolvers = new();
    private readonly Dictionary<string, Subscriber> _subscribers = new();

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

    public IEnumerable<string> GetFields()
    {
        foreach (var (field, _) in _resolvers) yield return field;

        foreach (var (field, _) in _subscribers)
        {
            if (_resolvers.ContainsKey(field))
                continue;

            yield return field;
        }
    }

    public void Add(string key, Resolver resolver)
    {
        _resolvers.Add(key, resolver);
    }

    public void Add(string key, Delegate resolver)
    {
        _resolvers.Add(key, DelegateResolverFactory.Get(resolver));
    }

    public void Add(string key, Subscriber subscriber)
    {
        _subscribers.Add(key, subscriber);
    }

    public void Add(string key, Subscriber subscriber, Resolver resolver)
    {
        if (key == null) throw new ArgumentNullException(nameof(key));
        if (subscriber == null) throw new ArgumentNullException(nameof(subscriber));
        if (resolver == null) throw new ArgumentNullException(nameof(resolver));

        _subscribers.Add(key, subscriber);
        _resolvers.Add(key, resolver);
    }

    public void Add(string key, Subscriber subscriber, Delegate resolver)
    {
        if (key == null) throw new ArgumentNullException(nameof(key));
        if (subscriber == null) throw new ArgumentNullException(nameof(subscriber));
        if (resolver == null) throw new ArgumentNullException(nameof(resolver));

        _subscribers.Add(key, subscriber);
        _resolvers.Add(key, DelegateResolverFactory.Get(resolver));
    }

    public Resolver? GetResolver(string key)
    {
        if (!_resolvers.ContainsKey(key))
            return null;

        return _resolvers[key];
    }

    public Subscriber? GetSubscriber(string key)
    {
        if (!_subscribers.ContainsKey(key))
            return null;

        return _subscribers[key];
    }

    public FieldResolversMap Clone()
    {
        var result = new FieldResolversMap();
        foreach (var aResolver in _resolvers) result._resolvers.Add(aResolver.Key, aResolver.Value);

        foreach (var aSubscriber in _subscribers) result._subscribers.Add(aSubscriber.Key, aSubscriber.Value);

        return result;
    }

    public static FieldResolversMap operator +(FieldResolversMap a, FieldResolversMap b)
    {
        var result = a.Clone();

        // override resolvers of a with b
        foreach (var bResolver in b._resolvers) result._resolvers[bResolver.Key] = bResolver.Value;

        foreach (var bSubscriber in b._subscribers) result._subscribers[bSubscriber.Key] = bSubscriber.Value;

        return result;
    }

    public static FieldResolversMap operator -(FieldResolversMap a, string fieldName)
    {
        var result = a.Clone();

        if (result._resolvers.ContainsKey(fieldName))
            result._resolvers.Remove(fieldName);

        if (result._subscribers.ContainsKey(fieldName))
            result._subscribers.Remove(fieldName);

        return result;
    }

    public void Replace(string fieldName, Resolver resolver)
    {
        _resolvers[fieldName] = resolver;
    }

    public void Replace(string fieldName, Subscriber subscriber)
    {
        _subscribers[fieldName] = subscriber;
    }

    public void RemoveResolver(string fieldName)
    {
        _resolvers.Remove(fieldName);
    }

    public void RemoveSubscriber(string fieldName)
    {
        _subscribers.Remove(fieldName);
    }
}