using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;

namespace Tanka.GraphQL.ValueResolution;

public class ResolverContextBase
{
    public required ObjectDefinition ObjectDefinition { get; set; }
    public required object? ObjectValue { get; set; }
    public required FieldDefinition Field { get; set; }
    public required FieldSelection Selection { get; set; }
    public required IReadOnlyCollection<FieldSelection> Fields { get; set; }
    public required IReadOnlyDictionary<string, object?> Arguments { get; set; }
    public required NodePath Path { get; set; }
    public required QueryContext QueryContext { get; set; }
    public ISchema Schema => QueryContext.Schema;
}

public class ResolverContext : ResolverContextBase
{
    public object? ResolvedValue { get; set; }

    public object? CompletedValue { get; set; }

    public Func<TypeDefinition, object?, TypeDefinition>? ResolveAbstractType { get; set; } = DefaultIsTypeOf;

    private static TypeDefinition DefaultIsTypeOf(TypeDefinition abstractType, object? value)
    {
        throw new InvalidOperationException(
            $"Cannot resolve actual type of value of abstract type {abstractType.Name}. " +
            "Use the context.IsTypeOf function to map from value to actual schema type");
    }
}