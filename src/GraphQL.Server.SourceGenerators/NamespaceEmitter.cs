using System.Collections.Generic;

using Microsoft.CodeAnalysis;

namespace Tanka.GraphQL.Server.SourceGenerators;

internal static class NamespaceEmitter
{
    public static void EmitNamespaceAddMethod(
        SourceProductionContext context,
        (string Namespace, List<(string Name, string Type)> Types) typesByNamespace)
    {
        string? nsName = typesByNamespace.Namespace;

        var builder = new IndentedStringBuilder();
        builder.AppendLine(ObjectTypeEmitter.NamespaceAddTemplate);

        if (!string.IsNullOrEmpty(nsName))
            builder.AppendLine($"namespace {nsName};");
        else
            nsName = "Global";

        string nsClassName = nsName.Replace(".", "");

        builder.AppendLine($"public static class {nsClassName}SourceGeneratedTypesExtensions");
        builder.AppendLine("{");
        builder.IncrementIndent();

        builder.AppendLine(
            $"public static SourceGeneratedTypesBuilder Add{nsClassName}Types(this SourceGeneratedTypesBuilder builder)");
        builder.AppendLine("{");
        builder.IncrementIndent();
        foreach (var (name, type) in typesByNamespace.Types)
        {
            if (type == "ObjectType")
                builder.AppendLine($"builder.Add{name}Controller();");
            else
                builder.AppendLine($"builder.Add{name}InputType();");

        }
        builder.AppendLine("return builder;");
        builder.DecrementIndent();
        builder.AppendLine("}");
        builder.DecrementIndent();
        builder.AppendLine("}");

        context.AddSource($"{nsName}.SourceGeneratedTypesExtensions.cs", builder.ToString());
    }
}