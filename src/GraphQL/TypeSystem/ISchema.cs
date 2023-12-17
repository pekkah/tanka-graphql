using Tanka.GraphQL.Language.Nodes.TypeSystem;
using Tanka.GraphQL.ValueResolution;
using Tanka.GraphQL.ValueSerialization;

namespace Tanka.GraphQL.TypeSystem;

public interface ISchema : IHasDirectives
{
    /// <summary>
    ///     Static schema instance with a query type containing no fields
    /// </summary>
    public static readonly ISchema Empty = new EmptySchema();

    /// <summary>
    ///     Optional root mutation type
    /// </summary>
    ObjectDefinition? Mutation { get; }

    /// <summary>
    ///     Root query type
    /// </summary>
    ObjectDefinition Query { get; }

    /// <summary>
    ///     Optional root subscription type
    /// </summary>
    ObjectDefinition? Subscription { get; }

    /// <summary>
    ///     Schema description
    /// </summary>
    string Description { get; }

    /// <summary>
    ///     Finds a named type from the schema
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    TypeDefinition? GetNamedType(string name);

    /// <summary>
    ///     Gets a field of type 
    /// </summary>
    /// <param name="type"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    FieldDefinition? GetField(string type, string name);

    /// <summary>
    ///     Get all fields of type
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    IEnumerable<KeyValuePair<string, FieldDefinition>> GetFields(string type);

    /// <summary>
    ///     Query types matching given filter and type
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="filter"></param>
    /// <returns></returns>
    IQueryable<T> QueryTypes<T>(Predicate<T>? filter = null) where T : TypeDefinition;

    /// <summary>
    ///     Get named directive type from schema
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    DirectiveDefinition? GetDirectiveType(string name);

    /// <summary>
    ///     Query directive types matching given filter
    /// </summary>
    /// <param name="filter"></param>
    /// <returns></returns>
    IQueryable<DirectiveDefinition> QueryDirectiveTypes(Predicate<DirectiveDefinition>? filter = null);

    /// <summary>
    ///     Get all input fields on an input object type
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    IEnumerable<KeyValuePair<string, InputValueDefinition>> GetInputFields(string type);

    /// <summary>
    ///     Get input field on an input object type
    /// </summary>
    /// <param name="type"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    InputValueDefinition? GetInputField(string type, string name);

    /// <summary>
    ///     Get possible types of an interface type
    /// </summary>
    /// <param name="abstractType"></param>
    /// <returns></returns>
    IEnumerable<TypeDefinition> GetPossibleTypes(InterfaceDefinition abstractType);

    /// <summary>
    ///     Get possible types of a union type
    /// </summary>
    /// <param name="abstractType"></param>
    /// <returns></returns>
    IEnumerable<ObjectDefinition> GetPossibleTypes(UnionDefinition abstractType);

    /// <summary>
    ///     Get resolver for a field of object type
    /// </summary>
    /// <param name="type"></param>
    /// <param name="fieldName"></param>
    /// <returns></returns>
    Resolver? GetResolver(string type, string fieldName);

    /// <summary>
    ///     Get subscriber for a field of object type
    /// </summary>
    /// <param name="type"></param>
    /// <param name="fieldName"></param>
    /// <returns></returns>
    Subscriber? GetSubscriber(string type, string fieldName);

    /// <summary>
    ///     Get value converter for a field of object type
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    IValueConverter? GetValueConverter(string type);
}