using System.Diagnostics;

using Tanka.GraphQL.Language.Nodes.TypeSystem;

namespace Tanka.GraphQL.ValueResolution;

public class ResolverContext : ResolverContextBase
{
    public ResolverContext()
    {
        ResolveAbstractType = DefaultIsTypeOf;
    }

    public object? ResolvedValue { get; set; }

    public object? CompletedValue { get; set; }

    public Func<TypeDefinition, object?, TypeDefinition>? ResolveAbstractType { get; set; }

    private TypeDefinition DefaultIsTypeOf(TypeDefinition abstractType, object? value)
    {
        if (value is INamedType namedType)
        {
            var typeDefinition = Schema.GetRequiredNamedType<TypeDefinition>(namedType.__Typename);
            ValidateAbstractType(abstractType, typeDefinition);
            return typeDefinition;
        }

        throw new InvalidOperationException(
            $"Cannot resolve actual type of value of abstract type {abstractType.Name}. " +
            "Use the context.ResolveAbstractType function to map from value to actual schema type");
    }

    [Conditional("DEBUG")]
    private void ValidateAbstractType(TypeDefinition abstractType, TypeDefinition typeDefinition)
    {
        IReadOnlyList<TypeDefinition> possibleTypes = abstractType switch
        {
            UnionDefinition unionDefinition => Schema.GetPossibleTypes(unionDefinition).ToList(),
            InterfaceDefinition interfaceDefinition => Schema.GetPossibleTypes(interfaceDefinition).ToList(),
            _ => throw new InvalidOperationException(
                $"Cannot resolve actual type of value of abstract type {abstractType.Name}. " +
                "Use the context.ResolveAbstractType function to map from value to actual schema type")
        };

        if (possibleTypes.Contains(typeDefinition)) return;

        throw new InvalidOperationException(
            $"Cannot resolve actual type of value of abstract type {abstractType.Name}. " +
            "Use the context.ResolveAbstractType function to map from value to actual schema type");
    }
}