using System.Collections;
using Tanka.GraphQL.Internal;

namespace Tanka.GraphQL.ValueResolution;

public class ResolversBuilder : IEnumerable
{
    private readonly Dictionary<string, Dictionary<string, ResolverBuilder>> _fieldResolvers = new();
    private readonly Dictionary<string, Dictionary<string, SubscriberBuilder>> _fieldSubscribers = new();

    protected ResolversBuilder(ResolversBuilder builder)
    {
        Properties = new CopyOnWriteDictionary<string, object?>(builder.Properties, StringComparer.Ordinal);
    }

    protected ResolversBuilder(IDictionary<string, object?> properties)
    {
        Properties = new CopyOnWriteDictionary<string, object?>(properties, StringComparer.Ordinal);
    }

    public ResolversBuilder()
    {
        Properties = new Dictionary<string, object?>(StringComparer.Ordinal);
    }

    public IDictionary<string, object?> Properties { get; }

    public IEnumerator GetEnumerator()
    {
        throw new NotImplementedException();
    }

    public void Add(string objectName, string fieldName, Action<ResolverBuilder> configure)
    {
        configure(Resolver(objectName, fieldName));
    }
    
    public void Add(string objectName, string fieldName,
        Action<SubscriberBuilder> configureSubscriber,
        Action<ResolverBuilder> configureResolver)
    {
        configureSubscriber(Subscriber(objectName, fieldName));
        configureResolver(Resolver(objectName, fieldName));
    }

    public ResolversBuilder Resolvers(string objectName, Dictionary<string, Action<ResolverBuilder>> resolvers)
    {
        foreach (var (fieldName, configureResolver) in resolvers) configureResolver(Resolver(objectName, fieldName));

        return this;
    }

    public ResolversBuilder Resolvers(string objectName, Dictionary<string, Resolver> resolvers)
    {
        foreach (var (fieldName, resolver) in resolvers) Resolver(objectName, fieldName).Run(resolver);

        return this;
    }

    public ResolversBuilder Resolvers(string objectName, Dictionary<string, Delegate> resolvers)
    {
        foreach (var (fieldName, resolver) in resolvers) 
            Resolver(objectName, fieldName).Run(resolver);

        return this;
    }

    public ResolverBuilder Resolver(string objectName, string fieldName)
    {
        if (!_fieldResolvers.TryGetValue(objectName, out var fields))
            fields = _fieldResolvers[objectName] = new();

        if (!fields.TryGetValue(fieldName, out var builder))
            builder = fields[fieldName] = new();

        return builder;
    }

    public SubscriberBuilder Subscriber(string objectName, string fieldName)
    {
        if (!_fieldSubscribers.TryGetValue(objectName, out var fields))
            fields = _fieldSubscribers[objectName] = new();

        if (!fields.TryGetValue(fieldName, out var builder))
            builder = fields[fieldName] = new();

        return builder;
    }

    public ResolversBuilder Subscriber(
        string objectName,
        string fieldName,
        Action<SubscriberBuilder> configureResolver)
    {
        configureResolver(Subscriber(objectName, fieldName));
        return this;
    }

    public SubscriberBuilder Subscriber(
        string objectName,
        string fieldName,
        Delegate subscriber)
    {
        return Subscriber(objectName, fieldName).Run(subscriber);
    }

    public bool HasResolver(string objectName, string fieldName)
    {
        return _fieldResolvers.TryGetValue(objectName, out var fields) && fields.ContainsKey(fieldName);
    }

    public bool HasSubscriber(string objectName, string fieldName)
    {
        return _fieldSubscribers.TryGetValue(objectName, out var fields) && fields.ContainsKey(fieldName);
    }

    protected T? GetProperty<T>(string key)
    {
        return Properties.TryGetValue(key, out var value) ? (T?)value : default;
    }

    protected void SetProperty<T>(string key, T value)
    {
        Properties[key] = value;
    }

    public IResolverMap BuildResolvers()
    {
        var resolvers = new DictionaryResolverMap(
            _fieldResolvers.ToDictionary(
                o => o.Key,
                o => o.Value.ToDictionary(f => f.Key, f => f.Value.Build())),
            new Dictionary<string, Dictionary<string, Subscriber>>(0));

        return resolvers;
    }

    public ISubscriberMap BuildSubscribers()
    {
        var subscribers = new DictionaryResolverMap(
            new Dictionary<string, Dictionary<string, Resolver>>(0),
            _fieldSubscribers.ToDictionary(
                o => o.Key,
                o => o.Value.ToDictionary(f => f.Key, f => f.Value.Build())));

        return subscribers;
    }

    public void RemoveResolver(string objectName, string fieldName)
    {
        if (!_fieldResolvers.TryGetValue(objectName, out var fields)) return;

        if (fields.ContainsKey(fieldName))
            fields.Remove(fieldName);
    }

    public void RemoveSubscriber(string objectName, string fieldName)
    {
        if (!_fieldSubscribers.TryGetValue(objectName, out var fields)) return;

        if (fields.ContainsKey(fieldName))
            fields.Remove(fieldName);
    }

    public void ReplaceResolver(string objectName, string fieldName, ResolverBuilder builder)
    {
        if (!_fieldResolvers.TryGetValue(objectName, out var fields))
            fields = _fieldResolvers[objectName] = new();

        fields[fieldName] = builder;
    }

    public void ReplaceSubscriber(string objectName, string fieldName, SubscriberBuilder builder)
    {
        if (!_fieldSubscribers.TryGetValue(objectName, out var fields))
            fields = _fieldSubscribers[objectName] = new();

        fields[fieldName] = builder;
    }
}