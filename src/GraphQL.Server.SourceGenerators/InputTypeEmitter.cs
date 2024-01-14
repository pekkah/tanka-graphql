using Microsoft.CodeAnalysis;
using System.Text;
using System.Text.Json;

namespace Tanka.GraphQL.Server.SourceGenerators;

public class InputTypeEmitter
{
    public const string ObjectTypeTemplate = """
        using System;
        using System.Threading.Tasks;
        using Microsoft.Extensions.Options;
        using Tanka.GraphQL.Server;
        using Tanka.GraphQL.Executable;
        using Tanka.GraphQL.ValueResolution;
        using Tanka.GraphQL.Fields;

        {{namespace}}

        public static class {{name}}InputTypeExtensions
        {
            public static SourceGeneratedTypesBuilder Add{{name}}InputType(
                this SourceGeneratedTypesBuilder builder)
            {
                builder.Builder.Configure(options => options.Builder.Add(
                        {{typeSDL}}
                        )
                    );

                return builder;
            }
        }
        
        """;

    public SourceProductionContext Context { get; }

    public InputTypeEmitter(SourceProductionContext context)
    {
        Context = context;
    }

    public void Emit(InputTypeDefinition definition)
    {
        var typeSDL = BuildTypeSdl(definition);

        var builder = new StringBuilder();
        string ns = string.IsNullOrEmpty(definition.Namespace) ? "" : $"{definition.Namespace}";
        builder.AppendLine(ObjectTypeTemplate
            .Replace("{{namespace}}", string.IsNullOrEmpty(ns) ? "" : $"namespace {ns};")
            .Replace("{{name}}", definition.TargetType)
            .Replace("{{typeSDL}}", typeSDL)
        );

        Context.AddSource($"{ns}{definition.TargetType}InputType.g.cs", builder.ToString());
    }

    private string BuildTypeSdl(InputTypeDefinition definition)
    {
        var builder = new IndentedStringBuilder();
        builder.AppendLine("\"\"\""); 
        builder.IndentCount = 4;
        builder.AppendLine($"input {definition.TargetType}");

        builder.AppendLine("{");
        
        using (builder.Indent())
        {
            foreach (var field in definition.Properties)
            {
                var fieldName = JsonNamingPolicy.CamelCase.ConvertName(field.Name);
                var fieldType = field.ClosestMatchingGraphQLTypeName;
                
                builder.AppendLine($"{fieldName}: {fieldType}");
            }
        }

        builder.AppendLine("}");
        builder.Append("\"\"\"");
        return builder.ToString();
    }
}