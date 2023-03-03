using Tanka.GraphQL.Language.Nodes.TypeSystem;

namespace Tanka.GraphQL.ValueResolution;

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