using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Tanka.GraphQL.Server.SourceGenerators;

public class TypeHelper
{
    public static string GetGraphQLTypeName(ITypeSymbol typeSymbol)
    {
        var nameAttribute = typeSymbol.GetAttributes()
            .FirstOrDefault(a => a.AttributeClass?.Name.StartsWith("GraphQLName") == true);

        if (nameAttribute is not null)
        {
            var name = nameAttribute.ConstructorArguments[0].Value?.ToString();
            if (name is not null)
            {
                return name;
            }
        }
        
        // Handle arrays
        if (typeSymbol is IArrayTypeSymbol arrayTypeSymbol)
        {
            return $"[{GetGraphQLTypeName(arrayTypeSymbol.ElementType)}]";
        }

        if (typeSymbol is INamedTypeSymbol { IsGenericType: true, ConstructedFrom.Name: "IAsyncEnumerable"} asyncEnumerable)
        {
            return GetGraphQLTypeName(asyncEnumerable.TypeArguments[0]);
        }

        if (typeSymbol is not { SpecialType: SpecialType.System_String })
        {
            var ienumerableT = typeSymbol
                .AllInterfaces
                .FirstOrDefault(i =>
                    i.OriginalDefinition.SpecialType == SpecialType.System_Collections_Generic_IEnumerable_T);

            if (ienumerableT is not null)
            {
                var innerType = ienumerableT.TypeArguments[0];
                return $"[{GetGraphQLTypeName(innerType)}]";
            }

            // Handle IEnumerable<T>
            if (typeSymbol is INamedTypeSymbol { IsGenericType: true, ConstructedFrom.Name: "IEnumerable" } namedType)
            {
                return $"[{GetGraphQLTypeName(namedType.TypeArguments[0])}]";
            }
        }

        bool isNullable = IsNullable(typeSymbol, out typeSymbol);

        string typeName;
        
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

    public static bool HasAttribute(
        SyntaxList<AttributeListSyntax> attributeLists,
        string searchAttributeName)
    {
        // Check if the parameter has any attributes
        if (!attributeLists.Any())
        {
            return false;
        }

        foreach (var attributeList in attributeLists)
        {
            foreach (var attribute in attributeList.Attributes)
            {
                var attributeName = attribute.Name.ToString();
                // should probably check fully qualified name
                var possibleNames = new[]
                {
                    attributeName,
                    attributeName.EndsWith("Attribute")
                        ? attributeName.Substring(0, attributeName.Length - 9)
                        : $"{attributeName}Attribute"
                };

                // should probably check fully qualified name
                if (possibleNames.Contains(searchAttributeName))
                {
                    return true;
                }
            }
        }

        return false;
    }

    public static bool HasAnyOfAttributes(
        SyntaxList<AttributeListSyntax> attributeLists,
        params string[] searchAttributeNames)
    {
        // Check if the parameter has any attributes
        if (!attributeLists.Any())
        {
            return false;
        }

        foreach (var attributeList in attributeLists)
        {
            foreach (var attribute in attributeList.Attributes)
            {
                var attributeName = attribute.Name.ToString();
                // should probably check fully qualified name
                var possibleNames = new[]
                {
                    attributeName,
                    attributeName.EndsWith("Attribute")
                        ? attributeName.Substring(0, attributeName.Length - 9)
                        : $"{attributeName}Attribute"
                };

                if (searchAttributeNames.Any(n => possibleNames.Contains(n)))
                {
                    return true;
                }
            }
        }

        return false;
    }

    public static bool IsPrimitiveType(TypeSyntax typeSyntax)
    {
        // Define a list of C# primitive types
        string[] primitiveTypes = new string[]
        {
            "bool",
            "byte",
            "sbyte",
            "char",
            "decimal",
            "double",
            "float",
            "int",
            "uint",
            "long",
            "ulong",
            "short",
            "ushort",
            "string"
        };

        // Get the name of the type syntax
        string typeName = UnwrapNullable(typeSyntax).ToString();

        // Check if the type name is a primitive type
        return primitiveTypes.Contains(typeName);
    }

    public static bool IsTypeNullable(TypeSyntax typeSyntax)
    {
        return typeSyntax.IsKind(SyntaxKind.NullableType);
    }

    public static TypeSyntax UnwrapTaskType(TypeSyntax typeSyntax)
    {
        // Check if the type is a named type syntax
        if (typeSyntax is GenericNameSyntax namedTypeSyntax)
        {
            // Check if the name of the type is "Task" or "ValueTask"
            if (namedTypeSyntax.Identifier.ValueText is "Task" or "ValueTask")
            {
                // Get the type argument list syntax for the named type syntax
                var typeArgumentListSyntax = namedTypeSyntax.TypeArgumentList;

                // If the type argument list syntax exists and contains an argument, return the type syntax for the argument
                if (typeArgumentListSyntax?.Arguments.Any() == true)
                {
                    return typeArgumentListSyntax.Arguments[0];
                }
            }
        }

        // If the type is not Task<T> or ValueTask<T>, return the type syntax itself
        return typeSyntax;
    }

    public static TypeSyntax UnwrapNullable(TypeSyntax typeSyntax)
    {
        if (typeSyntax is NullableTypeSyntax namedTypeSyntax)
        {
            return namedTypeSyntax.ElementType;
        }

        return typeSyntax;
    }

    public static bool IsValueTaskOrTask(TypeSyntax typeSyntax)
    {
        // Check if the type is a named type syntax
        if (typeSyntax is GenericNameSyntax namedTypeSyntax)
        {
            // Check if the name of the type is "ValueTask" or "Task"
            if (namedTypeSyntax.Identifier.ValueText is "ValueTask" or "Task")
            {
                // Check if the named type syntax has type arguments
                var typeArgumentListSyntax = namedTypeSyntax.TypeArgumentList;
                if (typeArgumentListSyntax?.Arguments.Any() == true)
                {
                    return true;
                }
            }
        }

        // If the type is not a named type syntax for ValueTask<T> or Task<T>, return false
        return false;
    }

    public static string GetNamespace(BaseTypeDeclarationSyntax syntax)
    {
        // If we don't have a namespace at all we'll return an empty string
        // This accounts for the "default namespace" case
        string nameSpace = string.Empty;

        // Get the containing syntax node for the type declaration
        // (could be a nested type, for example)
        SyntaxNode? potentialNamespaceParent = syntax.Parent;

        // Keep moving "out" of nested classes etc until we get to a namespace
        // or until we run out of parents
        while (potentialNamespaceParent != null &&
               potentialNamespaceParent is not NamespaceDeclarationSyntax
               && potentialNamespaceParent is not FileScopedNamespaceDeclarationSyntax)
        {
            potentialNamespaceParent = potentialNamespaceParent.Parent;
        }

        // Build up the final namespace by looping until we no longer have a namespace declaration
        if (potentialNamespaceParent is BaseNamespaceDeclarationSyntax namespaceParent)
        {
            // We have a namespace. Use that as the type
            nameSpace = namespaceParent.Name.ToString();

            // Keep moving "out" of the namespace declarations until we 
            // run out of nested namespace declarations
            while (true)
            {
                if (namespaceParent.Parent is not NamespaceDeclarationSyntax parent)
                {
                    break;
                }

                // Add the outer namespace as a prefix to the final namespace
                nameSpace = $"{namespaceParent.Name}.{nameSpace}";
                namespaceParent = parent;
            }
        }

        // return the final namespace
        return nameSpace;
    }

    public static ParentClass? GetParentClasses(BaseTypeDeclarationSyntax typeSyntax)
    {
        // Try and get the parent syntax. If it isn't a type like class/struct, this will be null
        var parentSyntax = typeSyntax.Parent as TypeDeclarationSyntax;
        ParentClass? parentClassInfo = null;

        // Keep looping while we're in a supported nested type
        while (parentSyntax != null && IsAllowedKind(parentSyntax.Kind()))
        {
            // Record the parent type keyword (class/struct etc), name, and constraints
            parentClassInfo = new ParentClass(
                parentSyntax.Keyword.ValueText,
                parentSyntax.Identifier.ToString() + parentSyntax.TypeParameterList,
                parentSyntax.ConstraintClauses.ToString(),
                parentClassInfo); // set the child link (null initially)

            // Move to the next outer type
            parentSyntax = parentSyntax.Parent as TypeDeclarationSyntax;
        }

        // return a link to the outermost parent type
        return parentClassInfo;
    }

    public static string GetResource(string nameSpace, ParentClass? parentClass, string resource)
    {
        var sb = new StringBuilder();

        // If we don't have a namespace, generate the code in the "default"
        // namespace, either global:: or a different <RootNamespace>
        bool hasNamespace = !string.IsNullOrEmpty(nameSpace);
        if (hasNamespace)
            // We could use a file-scoped namespace here which would be a little impler, 
            // but that requires C# 10, which might not be available. 
            // Depends what you want to support!
            sb
                .Append("namespace ")
                .Append(nameSpace)
                .AppendLine(@"
    {");

        var parentsCount = 0;
        // Loop through the full parent type hiearchy, starting with the outermost
        while (parentClass is not null)
        {
            sb
                .Append("    partial ")
                .Append(parentClass.Keyword) // e.g. class/struct/record
                .Append(' ')
                .Append(parentClass.Name) // e.g. Outer/Generic<T>
                .Append(' ')
                .Append(parentClass.Constraints) // e.g. where T: new()
                .AppendLine(@"
        {");
            parentsCount++; // keep track of how many layers deep we are
            parentClass = parentClass.Child; // repeat with the next child
        }

        // Write the actual target code
        sb.AppendLine(resource);

        // We need to "close" each of the parent types, so write
        // the required number of '}'
        for (var i = 0; i < parentsCount; i++) sb.AppendLine(@"    }");

        // Close the namespace, if we had one
        if (hasNamespace) sb.Append('}').AppendLine();

        return sb.ToString();
    }

    public static bool IsAllowedKind(SyntaxKind kind)
    {
        return kind == SyntaxKind.ClassDeclaration ||
               kind == SyntaxKind.StructDeclaration ||
               kind == SyntaxKind.RecordDeclaration;
    }

    public static IReadOnlyList<string> GetUsings(BaseTypeDeclarationSyntax classDeclaration)
    {
        return classDeclaration.SyntaxTree.GetCompilationUnitRoot()
            .Usings
            .Select(u => u.ToString())
            .ToList();
    }
}