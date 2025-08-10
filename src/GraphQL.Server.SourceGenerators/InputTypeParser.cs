using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Tanka.GraphQL.Server.SourceGenerators;

public class InputTypeParser(GeneratorAttributeSyntaxContext context)
{
    public GeneratorAttributeSyntaxContext Context { get; } = context;

    public InputTypeDefinition ParseInputTypeDefinition(ClassDeclarationSyntax classDeclaration)
    {
        var properties = ParseMembers(classDeclaration);

        return new InputTypeDefinition()
        {
            Namespace = TypeHelper.GetNamespace(classDeclaration),
            TargetType = classDeclaration.Identifier.Text,
            GraphQLName = NamedTypeExtension.GetName(context.SemanticModel, classDeclaration),
            Properties = properties,
            ParentClass = TypeHelper.GetParentClasses(classDeclaration)
        };
    }

    private List<ObjectPropertyDefinition> ParseMembers(ClassDeclarationSyntax classDeclaration)
    {
        var properties = new List<ObjectPropertyDefinition>();

        foreach (MemberDeclarationSyntax memberDeclarationSyntax in classDeclaration
                     .Members
                     .Where(m => ((SyntaxTokenList)m.Modifiers).Any(SyntaxKind.PublicKeyword)))
        {
            if (memberDeclarationSyntax.IsKind(SyntaxKind.PropertyDeclaration))
            {
                var propertyDeclaration = (PropertyDeclarationSyntax)memberDeclarationSyntax;
                var typeSymbol = Context.SemanticModel.GetTypeInfo(propertyDeclaration.Type).Type;
                var propertyDefinition = new ObjectPropertyDefinition()
                {
                    Name = propertyDeclaration.Identifier.Text,
                    ReturnType = propertyDeclaration.Type.ToString(),
                    ClosestMatchingGraphQLTypeName = GetClosestMatchingGraphQLTypeName(TypeHelper.UnwrapTaskType(propertyDeclaration.Type)),
                    ReturnTypeObject = typeSymbol != null ? TryParseInputTypeDefinition(typeSymbol) : null
                };
                properties.Add(propertyDefinition);
            }
        }

        return properties;
    }

    private InputTypeDefinition? TryParseInputTypeDefinition(ITypeSymbol namedTypeSymbol)
    {
        if (namedTypeSymbol.TypeKind != TypeKind.Class)
            return null;

        if (namedTypeSymbol.SpecialType is not SpecialType.None)
            return null;

        var properties = GetPublicProperties(namedTypeSymbol)
            .Select(property => new ObjectPropertyDefinition()
            {
                Name = property.Name,
                ReturnType = property.Type.ToString(),
                ClosestMatchingGraphQLTypeName = GetClosestMatchingGraphQLTypeName(property.Type),
            })
            .ToList();

        return new InputTypeDefinition() { TargetType = namedTypeSymbol.Name, Properties = properties };

        static IEnumerable<IPropertySymbol> GetPublicProperties(ITypeSymbol typeSymbol)
        {
            return typeSymbol.GetMembers()
                .Where(member => member.Kind == SymbolKind.Property)
                .Cast<IPropertySymbol>()
                .Where(property => property.DeclaredAccessibility == Accessibility.Public);
        }
    }

    private string GetClosestMatchingGraphQLTypeName(ITypeSymbol typeSymbol)
    {
        var typeName = TypeHelper.GetGraphQLTypeName(typeSymbol);
        return typeName;
    }


    private string GetClosestMatchingGraphQLTypeName(TypeSyntax typeSyntax)
    {
        TypeInfo typeInfo = Context.SemanticModel.GetTypeInfo(typeSyntax);

        var typeSymbol = typeInfo.Type;

        if (typeSymbol is null)
            return typeSyntax.ToString();

        return TypeHelper.GetGraphQLTypeName(typeSymbol, typeSyntax);
    }
}