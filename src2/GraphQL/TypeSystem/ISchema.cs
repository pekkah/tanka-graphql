using Tanka.GraphQL.Language.Nodes.TypeSystem;
using Tanka.GraphQL.ValueResolution;
using Tanka.GraphQL.ValueSerialization;

namespace Tanka.GraphQL.TypeSystem;

public interface ISchema : IHasDirectives
{
    public static readonly ISchema Empty = new EmptySchema();

    ObjectDefinition? Mutation { get; }

    ObjectDefinition Query { get; }

    ObjectDefinition? Subscription { get; }

    string Description { get; }

    TypeDefinition? GetNamedType(string name);

    FieldDefinition? GetField(string type, string name);

    IEnumerable<KeyValuePair<string, FieldDefinition>> GetFields(string type);

    IQueryable<T> QueryTypes<T>(Predicate<T>? filter = null) where T : TypeDefinition;

    DirectiveDefinition? GetDirectiveType(string name);

    IQueryable<DirectiveDefinition> QueryDirectiveTypes(Predicate<DirectiveDefinition>? filter = null);

    IEnumerable<KeyValuePair<string, InputValueDefinition>> GetInputFields(string type);

    InputValueDefinition? GetInputField(string type, string name);

    IEnumerable<TypeDefinition> GetPossibleTypes(InterfaceDefinition abstractType);

    IEnumerable<ObjectDefinition> GetPossibleTypes(UnionDefinition abstractType);

    Resolver? GetResolver(string type, string fieldName);

    Subscriber? GetSubscriber(string type, string fieldName);

    IValueConverter? GetValueConverter(string type);
}