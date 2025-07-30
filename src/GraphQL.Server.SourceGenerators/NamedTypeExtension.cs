using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Scriban;
using Scriban.Runtime;

namespace Tanka.GraphQL.Server.SourceGenerators;

public static class NamedTypeExtension
{
    public static string GraphQLNamedTypeTemplate =>
          """
          {{~ if is_static ~}}
          public static partial {{type}} {{name}}
          {
              public static string __Typename => "{{type_name}}";
          }
          {{~ else ~}}
          public partial {{type}} {{name}}: INamedType
          {
              public string __Typename => "{{type_name}}";
          }
          {{~ end ~}}
          """;

    public static string Render(string type, string name, string graphQLName, bool isStatic = false)
    {
        var template = Template.Parse(GraphQLNamedTypeTemplate);
        var scriptObject = new ScriptObject();
        scriptObject.Import(new { type, name, typeName = graphQLName, isStatic });
        return template.Render(scriptObject);
    }

    public static string GetName(SemanticModel semanticModel, TypeDeclarationSyntax typeDeclaration)
    {
        var name = typeDeclaration.Identifier.Text;
        var graphQLName = name;
        var graphQLNameAttribute = typeDeclaration.AttributeLists
            .SelectMany(list => list.Attributes)
            .FirstOrDefault(attribute => attribute.Name.ToString() == "GraphQLName");

        if (graphQLNameAttribute != null)
        {
            var graphQLNameArgument = graphQLNameAttribute.ArgumentList?.Arguments.FirstOrDefault();
            if (graphQLNameArgument != null)
            {
                graphQLName = (string)semanticModel.GetOperation(graphQLNameArgument.Expression)!.ConstantValue!.Value!;
            }
        }

        return graphQLName;
    }

    public static string GetName(INamedTypeSymbol namedType)
    {
        var graphQLNameAttribute = namedType.GetAttributes()
            .FirstOrDefault(attribute => attribute.AttributeClass?.Name == "GraphQLNameAttribute");

        if (graphQLNameAttribute != null)
        {
            var graphQLName = (string?)graphQLNameAttribute.ConstructorArguments[0].Value ?? string.Empty;

            if (!string.IsNullOrWhiteSpace(graphQLName))
                return graphQLName;
        }

        return namedType.Name;
    }
}