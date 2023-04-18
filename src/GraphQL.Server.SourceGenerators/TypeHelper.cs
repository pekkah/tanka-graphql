using Microsoft.CodeAnalysis;
using System.Linq;

namespace Tanka.GraphQL.Server.SourceGenerators;

public class TypeHelper
{
    public static string GetGraphQLTypeName(ITypeSymbol typeSymbol)
    {
        // Handle arrays
        if (typeSymbol is IArrayTypeSymbol arrayTypeSymbol)
        {
            return $"[{GetGraphQLTypeName(arrayTypeSymbol.ElementType)}]";
        }

        // Handle IEnumerable<T>
        if (typeSymbol is INamedTypeSymbol { IsGenericType: true, ConstructedFrom.Name: "IEnumerable" } namedType)
        {
            return $"[{GetGraphQLTypeName(namedType.TypeArguments[0])}]";
        }

        bool isNullable = IsNullable(typeSymbol, out typeSymbol);

        var typeName = string.Empty;
        
        // Handle primitive types
        if (typeSymbol.SpecialType != SpecialType.None && typeSymbol.SpecialType != SpecialType.System_Void)
        {
            switch (typeSymbol.SpecialType)
            {
                case SpecialType.System_Boolean:
                    typeName = "Boolean";
                    break;
                case SpecialType.System_SByte:
                case SpecialType.System_Byte:
                case SpecialType.System_Int16:
                case SpecialType.System_UInt16:
                case SpecialType.System_Int32:
                case SpecialType.System_UInt32:
                case SpecialType.System_Int64:
                case SpecialType.System_UInt64:
                    typeName = "Int";
                    break;
                case SpecialType.System_Single:
                case SpecialType.System_Double:
                    typeName = "Float";
                    break;
                case SpecialType.System_Char:
                case SpecialType.System_String:
                    typeName = "String";
                    break;
                default:
                    return typeSymbol.Name;
            }
        }
        else
        {
            typeName = typeSymbol.Name;
        }

        if (!isNullable)
        {
            typeName = string.Concat(typeName, "!");
        }

        return typeName;

    }

    public static bool IsNullable(ITypeSymbol typeSymbol, out ITypeSymbol innerType)
    {
        if (typeSymbol is INamedTypeSymbol
            {
                OriginalDefinition.SpecialType: SpecialType.System_Nullable_T
            } namedTypeSymbol)
        {
            innerType = namedTypeSymbol.TypeArguments[0];
            return true;
        }

        innerType = typeSymbol;
        return typeSymbol.NullableAnnotation == NullableAnnotation.Annotated;
    }

    private static bool IsPrimitive(ITypeSymbol typeSymbol)
    {
        if (typeSymbol is INamedTypeSymbol
            {
                OriginalDefinition.SpecialType: SpecialType.System_Nullable_T
            } namedTypeSymbol)
            // The parameter is nullable, get the underlying type
            typeSymbol = namedTypeSymbol.TypeArguments[0];

        if (typeSymbol.SpecialType != SpecialType.None &&
            typeSymbol.SpecialType != SpecialType.System_Object)
            return true;

        return false;
    }

    private static bool IsTaskOrValueTask(ITypeSymbol typeSymbol)
    {
        return typeSymbol.Name is "Task" or "ValueTask"
               && typeSymbol.ContainingNamespace.ToDisplayString() == "System.Threading.Tasks";
    }

    private static bool IsTaskOrValueTaskOfT(ITypeSymbol typeSymbol)
    {
        if (typeSymbol is INamedTypeSymbol namedTypeSymbol)
            // Check if the type is Task<T> or ValueTask<T>
            if (namedTypeSymbol.TypeArguments.Length == 1 &&
                (namedTypeSymbol.Name == "Task" || namedTypeSymbol.Name == "ValueTask") &&
                namedTypeSymbol.ContainingNamespace.ToDisplayString() == "System.Threading.Tasks")
                return true;

        return false;
    }

    public static bool ValidateReturnType(ITypeSymbol returnTypeSymbol)
    {
        return returnTypeSymbol.SpecialType != SpecialType.System_Void &&
               !IsTaskOrValueTask(returnTypeSymbol);
    }
}