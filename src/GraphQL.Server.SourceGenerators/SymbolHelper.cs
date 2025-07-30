using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Tanka.GraphQL.Server.SourceGenerators.Internal;

namespace Tanka.GraphQL.Server.SourceGenerators;

public static class SymbolHelper
{
    public static (List<ObjectPropertyDefinition> Properties, List<ObjectMethodDefinition> Methods) ParseMembers(
        INamedTypeSymbol classSymbol)
    {
        var properties = new List<ObjectPropertyDefinition>();
        var methods = new List<ObjectMethodDefinition>();

        foreach (ISymbol memberSymbol in classSymbol
                     .GetMembers()
                     .Where(m => m.DeclaredAccessibility == Accessibility.Public))
        {
            if (memberSymbol is IPropertySymbol property)
            {
                var propertyDefinition = new ObjectPropertyDefinition()
                {
                    IsStatic = property.IsStatic,
                    Name = property.Name,
                    ReturnType = property.Type.ToString(),
                    ClosestMatchingGraphQLTypeName = GetClosestMatchingGraphQLTypeName(property.Type),
                };
                properties.Add(propertyDefinition);
            }
            else if (memberSymbol is IMethodSymbol { MethodKind: not (MethodKind.PropertyGet or MethodKind.PropertySet) } method)
            {
                var methodDefinition = new ObjectMethodDefinition()
                {
                    IsStatic = method.IsStatic,
                    Name = method.Name,
                    ReturnType = UnwrapTaskType(method.ReturnType).ToString(),
                    ClosestMatchingGraphQLTypeName = GetClosestMatchingGraphQLTypeName(method.ReturnType),
                    Type = GetMethodType(method),
                    Parameters = method.Parameters
                        .Select(p => new ParameterDefinition()
                        {
                            Name = p.Name,
                            Type = p.Type.ToString(),
                            ClosestMatchingGraphQLTypeName = GetClosestMatchingGraphQLTypeName(UnwrapTaskType(p.Type)),
                            IsNullable = p.NullableAnnotation == NullableAnnotation.Annotated,
                            IsPrimitive = IsPrimitiveType(p.Type!),
                            FromArguments = HasAttribute(p, "FromArgumentsAttribute"),
                            FromServices = HasAttribute(p, "FromServicesAttribute")
                        }).ToEquatableArray()
                };
                methods.Add(methodDefinition);
            }
        }

        return (properties, methods);
    }

    public static bool HasAttribute(ISymbol symbol, string attributeName)
    {
        return symbol.GetAttributes()
            .Any(attr => attr.AttributeClass?.Name == attributeName
                         && attr.AttributeClass.ContainingNamespace.ToDisplayString().StartsWith("Tanka"));
    }


    public static bool IsPrimitiveType(ITypeSymbol typeSymbol)
    {
        // Define a list of C# primitive types
        SpecialType[] primitiveTypes = new SpecialType[]
        {
            SpecialType.System_Boolean,
            SpecialType.System_Byte,
            SpecialType.System_SByte,
            SpecialType.System_Char,
            SpecialType.System_Decimal,
            SpecialType.System_Double,
            SpecialType.System_Single, // float
            SpecialType.System_Int32,
            SpecialType.System_UInt32,
            SpecialType.System_Int64,
            SpecialType.System_UInt64,
            SpecialType.System_Int16,
            SpecialType.System_UInt16,
            SpecialType.System_String
        };

        // Check if the type symbol's special type is a primitive type
        return primitiveTypes.Contains(typeSymbol.SpecialType);
    }


    private static ITypeSymbol UnwrapTaskType(ITypeSymbol possibleTaskType)
    {
        if (possibleTaskType is INamedTypeSymbol namedType)
        {
            if (namedType.ConstructedFrom?.ToString() == "System.Threading.Tasks.Task`1" ||
                namedType.ConstructedFrom?.ToString() == "System.Threading.Tasks.ValueTask`1" ||
                namedType.ConstructedFrom?.ToString() == "System.Collections.Generic.IAsyncEnumerable`1")
            {
                return namedType.TypeArguments[0];
            }
        }

        return possibleTaskType;
    }

    private static string GetClosestMatchingGraphQLTypeName(ITypeSymbol type)
    {
        return TypeHelper.GetGraphQLTypeName(UnwrapTaskType(type));
    }

    private static MethodType GetMethodType(IMethodSymbol method)
    {
        var returnType = method.ReturnType;

        if (returnType.SpecialType == SpecialType.System_Void)
            return MethodType.Void;

        if (returnType is INamedTypeSymbol namedType)
        {
            switch (namedType.ConstructedFrom?.ToString())
            {
                case "System.Threading.Tasks.Task":
                    return namedType.TypeArguments.Length == 0 ? MethodType.Task : MethodType.TaskOfT;
                case "System.Threading.Tasks.ValueTask":
                    return namedType.TypeArguments.Length == 0 ? MethodType.ValueTask : MethodType.ValueTaskOfT;
                case "System.Collections.Generic.IAsyncEnumerable`1":
                    return MethodType.AsyncEnumerableOfT;
                case "System.Collections.Generic.IEnumerable`1":
                    return MethodType.EnumerableT;
                default:
                    return MethodType.T;
            }
        }

        return MethodType.Unknown;
    }

    public static IReadOnlyList<BaseDefinition> GetImplements(INamedTypeSymbol namedTypeSymbol)
    {
        var baseDefinitions = new List<BaseDefinition>();

        /*if (namedTypeSymbol.BaseType != null && namedTypeSymbol.BaseType is not { SpecialType: SpecialType.System_Object })
        {
            baseDefinitions.Add(
                GetBaseDefinition(namedTypeSymbol.BaseType)
                );
        }*/

        baseDefinitions.AddRange(
            namedTypeSymbol
                .Interfaces
                .Where(i => HasAttribute(i, "InterfaceTypeAttribute"))
                .Select(GetBaseDefinition)
        );

        return baseDefinitions;
    }

    public static BaseDefinition GetBaseDefinition(INamedTypeSymbol baseNamedTypeSymbol)
    {
        var (properties, methods) = SymbolHelper.ParseMembers(baseNamedTypeSymbol);

        return new BaseDefinition(
            baseNamedTypeSymbol.TypeKind == TypeKind.Class,
            baseNamedTypeSymbol.Name,
            baseNamedTypeSymbol.ContainingNamespace.ToDisplayString(),
            NamedTypeExtension.GetName(baseNamedTypeSymbol),
            properties,
            methods
        );
    }

}