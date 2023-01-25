using Tanka.GraphQL.Fields;
using Tanka.GraphQL.Language;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;
using Tanka.GraphQL.ValueResolution;
using Tanka.GraphQL.ValueSerialization;

namespace Tanka.GraphQL.TypeSystem;

public class ExecutableSchema : ISchema
{
    private readonly DirectiveList _directives;
    private readonly IReadOnlyDictionary<string, DirectiveDefinition> _directiveTypes;
    private readonly IReadOnlyDictionary<string, Dictionary<string, FieldDefinition>> _fields;
    private readonly IReadOnlyDictionary<string, Dictionary<string, InputValueDefinition>> _inputFields;
    private readonly IResolverMap _resolvers;
    private readonly IReadOnlyDictionary<string, IValueConverter> _scalarSerializers;
    private readonly ISubscriberMap _subscribers;
    private readonly IReadOnlyDictionary<string, TypeDefinition> _types;

    public ExecutableSchema(
        IReadOnlyDictionary<string, TypeDefinition> types,
        IReadOnlyDictionary<string, Dictionary<string, FieldDefinition>> fields,
        IReadOnlyDictionary<string, Dictionary<string, InputValueDefinition>> inputFields,
        IReadOnlyDictionary<string, DirectiveDefinition> directiveTypes,
        ObjectDefinition queryRoot,
        IResolverMap resolvers,
        IReadOnlyDictionary<string, IValueConverter> scalarSerializers,
        ObjectDefinition? mutationRoot = null,
        ObjectDefinition? subscriptionRoot = null,
        ISubscriberMap? subscribers = null,
        IEnumerable<Directive>? directives = null)
    {
        _types = types;
        _fields = fields;
        _inputFields = inputFields;
        _directiveTypes = directiveTypes;
        _resolvers = resolvers;
        _subscribers = subscribers;
        _scalarSerializers = scalarSerializers;
        _directives = new(directives);

        Query = queryRoot;
        Mutation = mutationRoot;
        Subscription = subscriptionRoot;
    }

    public ObjectDefinition? Subscription { get; }

    public string Description { get; } = "Root types";

    public ObjectDefinition Query { get; }

    public ObjectDefinition? Mutation { get; }

    public TypeDefinition? GetNamedType(string name)
    {
        if (_types.TryGetValue(name, out var type)) return type;

        return null;
    }

    public FieldDefinition? GetField(string type, string name)
    {
        if (_fields.TryGetValue(type, out var fields))
            if (fields.TryGetValue(name, out var field))
                return field;

        return null;
    }

    public IEnumerable<KeyValuePair<string, FieldDefinition>> GetFields(string type)
    {
        if (_fields.TryGetValue(type, out var fields)) return fields;

        return Enumerable.Empty<KeyValuePair<string, FieldDefinition>>();
    }

    public IQueryable<T> QueryTypes<T>(Predicate<T>? filter = null) where T : TypeDefinition
    {
        if (filter == null)
            return _types.Select(t => t.Value)
                .OfType<T>()
                .AsQueryable();

        return _types.Select(t => t.Value)
            .OfType<T>()
            .Where(t => filter(t))
            .AsQueryable();
    }

    public DirectiveDefinition? GetDirectiveType(string name)
    {
        if (_directiveTypes.TryGetValue(name, out var directive)) return directive;

        return null;
    }

    public IQueryable<DirectiveDefinition> QueryDirectiveTypes(
        Predicate<DirectiveDefinition>? filter = null)
    {
        if (filter != null)
            return _directiveTypes.Select(v => v.Value)
                .Where(d => filter(d))
                .AsQueryable();

        return _directiveTypes.Select(v => v.Value).AsQueryable();
    }

    public IEnumerable<KeyValuePair<string, InputValueDefinition>> GetInputFields(string type)
    {
        if (_inputFields.TryGetValue(type, out var fields)) return fields;

        return Enumerable.Empty<KeyValuePair<string, InputValueDefinition>>();
    }

    public InputValueDefinition? GetInputField(string type, string name)
    {
        if (_inputFields.TryGetValue(type, out var fields))
            if (fields.TryGetValue(name, out var field))
                return field;

        return null;
    }

    public IEnumerable<TypeDefinition> GetPossibleTypes(InterfaceDefinition abstractType)
    {
        foreach (var objectDefinition in QueryTypes<ObjectDefinition>(ob => ob.HasInterface(abstractType.Name)))
            yield return objectDefinition;

        foreach (var interfaceDefinition in QueryTypes<InterfaceDefinition>(ob => ob.HasInterface(abstractType.Name)))
            yield return interfaceDefinition;
    }

    public IEnumerable<ObjectDefinition> GetPossibleTypes(UnionDefinition abstractType)
    {
        return QueryTypes<ObjectDefinition>(ob => abstractType.HasMember(ob.Name));
    }

    public Resolver? GetResolver(string type, string fieldName)
    {
        return _resolvers.GetResolver(type, fieldName);
    }

    public Subscriber? GetSubscriber(string type, string fieldName)
    {
        return _subscribers.GetSubscriber(type, fieldName);
    }

    public IValueConverter? GetValueConverter(string type)
    {
        if (_scalarSerializers.TryGetValue(type, out var serializer))
            return serializer;

        return null;
    }

    public IEnumerable<Directive> Directives => _directives;

    public Directive? GetDirective(string name)
    {
        return null;
    }

    public bool HasDirective(string name)
    {
        return _directives.HasDirective(name);
    }

    public T? GetNamedType<T>(string name) where T : TypeDefinition
    {
        return (T?)GetNamedType(name);
    }
}