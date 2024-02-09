using Microsoft.CodeAnalysis;

using Tanka.GraphQL.Server.SourceGenerators.Templates;

namespace Tanka.GraphQL.Server.SourceGenerators;

public class ObjectTypeEmitter
{
    public static void Emit(
        SourceProductionContext context,
        ObjectControllerDefinition definition)
    {
        string ns = string.IsNullOrEmpty(definition.Namespace) ? "" : $"{definition.Namespace}";
        var template = new ObjectTemplate
        {
            Namespace = ns,
            Name = definition.TargetType,
            TypeName = definition.GraphQLName ?? definition.TargetType,
            Methods = definition.Methods,
            Properties = definition.Properties,
            Usings = definition.Usings,
            NamedTypeExtension = NamedTypeExtension.Render( 
                    "class", 
                    definition.TargetType,
                        definition.GraphQLName ?? definition.TargetType,
                        definition.IsStatic)
        };
        string content = template.Render();
        context.AddSource($"{ns}{definition.TargetType}Controller.g.cs", content);
    }
}