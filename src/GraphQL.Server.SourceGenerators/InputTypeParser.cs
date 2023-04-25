using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CSharpExtensions = Microsoft.CodeAnalysis.CSharpExtensions;

namespace Tanka.GraphQL.Server.SourceGenerators;

public class InputTypeParser
{
    public GeneratorAttributeSyntaxContext Context { get; }

    public InputTypeParser(GeneratorAttributeSyntaxContext context)
    {
        Context = context;
    }

    public InputTypeDefinition ParseInputTypeDefinition(ClassDeclarationSyntax classDeclaration)
    {
        var properties = ParseMembers(classDeclaration);

        return new InputTypeDefinition()
        {
            Namespace = TypeHelper.GetNamespace(classDeclaration),
            TargetType = classDeclaration.Identifier.Text,
            Properties = properties,
            ParentClass = TypeHelper.GetParentClasses(classDeclaration)
        };
    }

    private  List<ObjectPropertyDefinition> ParseMembers(ClassDeclarationSyntax classDeclaration)
    {
        var properties = new List<ObjectPropertyDefinition>();

        foreach (MemberDeclarationSyntax memberDeclarationSyntax in classDeclaration
                     .Members
                     .Where(m => CSharpExtensions.Any((SyntaxTokenList)m.Modifiers, SyntaxKind.PublicKeyword)))
        {
            if (memberDeclarationSyntax.IsKind(SyntaxKind.PropertyDeclaration))
            {
                var propertyDeclaration = (PropertyDeclarationSyntax)memberDeclarationSyntax;
                var propertyDefinition = new ObjectPropertyDefinition()
                {
                    Name = propertyDeclaration.Identifier.Text,
                    ReturnType = propertyDeclaration.Type.ToString(),
                    ClosestMatchingGraphQLTypeName = GetClosestMatchingGraphQLTypeName(TypeHelper.UnwrapTaskType(propertyDeclaration.Type)),
                    IsNullable = TypeHelper.IsTypeNullable(propertyDeclaration.Type),
                };
                properties.Add(propertyDefinition);
            }
        }

        return properties;
    }

    private string GetClosestMatchingGraphQLTypeName(TypeSyntax typeSyntax)
    {
        var typeSymbol = Context.SemanticModel.GetTypeInfo(typeSyntax).Type;

        if (typeSymbol is null)
            return typeSyntax.ToString();

        return TypeHelper.GetGraphQLTypeName(typeSymbol);
    }
}