namespace Tanka.GraphQL.ValueResolution;

internal class DictionaryResolverMap : IResolverMap, ISubscriberMap
{
    private readonly IDictionary<string, Dictionary<string, Resolver>> _resolvers;
    private readonly IDictionary<string, Dictionary<string, Subscriber>> _subscribers;

    public DictionaryResolverMap(
        IDictionary<string, Dictionary<string, Resolver>> resolvers,
        IDictionary<string, Dictionary<string, Subscriber>> subscribers)
    {
        _resolvers = resolvers;
        _subscribers = subscribers;
    }

    public Resolver? GetResolver(string typeName, string fieldName)
    {
        if (!_resolvers.TryGetValue(typeName, out var fields)) return null;

        return fields.TryGetValue(fieldName, out var resolver) ? resolver : null;
    }

    public IEnumerable<(string TypeName, IEnumerable<string> Fields)> GetTypes()
    {
        foreach (var (typeName, fields) in _resolvers)
            yield return (typeName, fields.Select(f => f.Key));
    }

    public Subscriber? GetSubscriber(string typeName, string fieldName)
    {
        if (!_subscribers.TryGetValue(typeName, out var fields)) return null;

        return fields.TryGetValue(fieldName, out var subscriber) ? subscriber : null;
    }
}