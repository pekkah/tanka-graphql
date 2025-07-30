using Microsoft.CodeAnalysis;

using Tanka.GraphQL.Server.SourceGenerators.Templates;

namespace Tanka.GraphQL.Server.SourceGenerators;

public class InterfaceTypeEmitter(SourceProductionContext context)
{
    public void Emit(InterfaceControllerDefinition definition)
    {
        string ns = string.IsNullOrEmpty(definition.Namespace) ? "" : $"{definition.Namespace}";
        var template = new InterfaceTemplate()
        {
            Namespace = ns,
            Name = definition.TargetType,
            TypeName = definition.GraphQLName ?? definition.TargetType,
            Methods = definition.Methods,
            Properties = definition.Properties,
            Usings = definition.Usings,
            NamedTypeExtension = NamedTypeExtension.Render(
                "interface",
                definition.TargetType,
                definition.GraphQLName ?? definition.TargetType,
                definition.IsStatic)
        };

        var content = template.Render();
        context.AddSource($"{ns}{definition.TargetType}Controller.g.cs", content);
    }
}