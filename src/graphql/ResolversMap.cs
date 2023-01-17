using Tanka.GraphQL.Fields;

namespace Tanka.GraphQL;

public class ResolversMap : Dictionary<string, FieldResolversMap>, IResolverMap, ISubscriberMap
{
    public ResolversMap(IResolverMap resolvers, ISubscriberMap? subscribers = null)
    {
        Add(resolvers, subscribers);
    }

    public ResolversMap()
    {
    }

    public static IResolverMap None { get; } = new ResolversMap();

    public Resolver? GetResolver(string typeName, string fieldName)
    {
        if (!TryGetValue(typeName, out var objectNode))
            return null;

        var resolver = objectNode.GetResolver(fieldName);
        return resolver;
    }

    public IEnumerable<(string TypeName, IEnumerable<string> Fields)> GetTypes()
    {
        foreach (var (typeName, fields) in this) yield return (typeName, fields.GetFields());
    }

    public Subscriber? GetSubscriber(string typeName, string fieldName)
    {
        if (!TryGetValue(typeName, out var objectNode))
            return null;

        var resolver = objectNode.GetSubscriber(fieldName);
        return resolver;
    }

    public void Add(IResolverMap resolvers, ISubscriberMap? subscribers)
    {
        foreach (var (typeName, fields) in resolvers.GetTypes())
        foreach (var field in fields)
        {
            var resolver = resolvers?.GetResolver(typeName, field);
            var subscriber = subscribers?.GetSubscriber(typeName, field);

            if (resolver is not null) Add(typeName, field, resolver, subscriber);
        }

        if (subscribers is not null)
            foreach (var (typeName, fields) in subscribers.GetTypes())
            foreach (var field in fields)
            {
                var resolver = resolvers?.GetResolver(typeName, field);
                var subscriber = subscribers?.GetSubscriber(typeName, field);

                if (subscriber is not null) Add(typeName, field, subscriber);
            }
    }

    public void Add(string typeName, string fieldName, Resolver resolver, Subscriber? subscriber = null)
    {
        if (!TryGetValue(typeName, out var fieldsResolvers)) fieldsResolvers = this[typeName] = new();

        if (subscriber is null)
            fieldsResolvers.Add(fieldName, resolver);
        else
            fieldsResolvers.Add(fieldName, subscriber, resolver);
    }

    public bool Add(string typeName, string fieldName, Subscriber subscriber)
    {
        if (!TryGetValue(typeName, out var fieldsResolvers)) fieldsResolvers = this[typeName] = new();

        if (fieldsResolvers.GetSubscriber(fieldName) is not null)
            return false;

        fieldsResolvers.Add(fieldName, subscriber);
        return true;
    }

    public void Replace(string typeName, string fieldName, Resolver resolver)
    {
        if (!TryGetValue(typeName, out var fieldsResolvers))
            fieldsResolvers = this[typeName] = new();

        if (fieldsResolvers.GetResolver(fieldName) is not null)
            fieldsResolvers.Replace(fieldName, resolver);
        else
            fieldsResolvers.Add(fieldName, resolver);
    }

    public void Replace(string typeName, string fieldName, Subscriber subscriber)
    {
        if (!TryGetValue(typeName, out var fieldsResolvers))
            fieldsResolvers = this[typeName] = new();

        if (fieldsResolvers.GetSubscriber(fieldName) is not null)
            fieldsResolvers.Replace(fieldName, subscriber);
        else
            fieldsResolvers.Add(fieldName, subscriber);
    }

    public void RemoveResolver(string typeName, string fieldName)
    {
        if (!TryGetValue(typeName, out var fieldsResolvers))
            return;

        if (fieldsResolvers.GetResolver(fieldName) is not null) fieldsResolvers.RemoveResolver(fieldName);
    }

    public void RemoveSubscriber(string typeName, string fieldName)
    {
        if (!TryGetValue(typeName, out var fieldsResolvers))
            return;

        if (fieldsResolvers.GetSubscriber(fieldName) is not null) fieldsResolvers.RemoveSubscriber(fieldName);
    }

    public ResolversMap Clone()
    {
        var result = new ResolversMap();

        foreach (var objectMap in this)
            result.Add(objectMap.Key, objectMap.Value.Clone());

        return result;
    }

    public static ResolversMap operator +(ResolversMap a, ResolversMap b)
    {
        var result = a.Clone();

        result.Add(b, b);

        return result;
    }

    public static ResolversMap operator +(ResolversMap a, (string Name, FieldResolversMap Fields) objectDefinition)
    {
        var result = a.Clone();

        if (result.ContainsKey(objectDefinition.Name))
            result[objectDefinition.Name] += objectDefinition.Fields;
        else
            result[objectDefinition.Name] = objectDefinition.Fields;

        return result;
    }

    public static ResolversMap operator -(ResolversMap a, string name)
    {
        var result = a.Clone();

        if (result.ContainsKey(name))
            result.Remove(name);

        return result;
    }

    public static ResolversMap operator -(ResolversMap a, ResolversMap b)
    {
        var result = a.Clone();

        // remove b by key
        foreach (var objectMap in b)
            if (result.ContainsKey(objectMap.Key))
                result.Remove(objectMap.Key);

        return result;
    }
}