using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Text.Json;
using Microsoft.CodeAnalysis;

namespace Tanka.GraphQL.Server.SourceGenerators;

public class ObjectTypeEmitter
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

        public static class {{name}}Controller
        {
        {{properties}}

        {{methods}}
        }

        public static class {{name}}ControllerExtensions
        {
            public static SourceGeneratedTypesBuilder Add{{name}}Controller(
                this SourceGeneratedTypesBuilder builder)
            {
                builder.Builder.Configure(options => options.Builder.Add(
                    "{{name}}",
                    new FieldsWithResolvers()
                    {
                        {{fieldsWithResolvers}}
                    }));

                return builder;
            }
        }
        """;

    public const string FieldWithResolverTemplate = """
        { "{{fieldName}}: {{fieldType}}", {{resolverMethod}} }
        """;

    public const string NamespaceAddTemplate = """
        using System;
        using System.Threading.Tasks;
        using Microsoft.Extensions.Options;
        using Tanka.GraphQL.Server;
        using Tanka.GraphQL.Executable;
        using Tanka.GraphQL.ValueResolution;
        using Tanka.GraphQL.Fields;
       
        """;


    public static void Emit(
        SourceProductionContext context,
        ObjectControllerDefinition definition)
    {
        string properties = EmitProperties(definition);
        string methods = EmitMethods(definition);
        string fieldsWithResolvers = EmitFieldsWithResolvers(definition);

        var builder = new StringBuilder();

        string ns = string.IsNullOrEmpty(definition.Namespace) ? "" : $"{definition.Namespace}";
        builder.AppendLine(ObjectTypeTemplate
            .Replace("{{properties}}", properties)
            .Replace("{{methods}}", methods)
            .Replace("{{namespace}}", string.IsNullOrEmpty(ns) ? "" : $"namespace {ns};")
            .Replace("{{name}}", definition.TargetType)
            .Replace("{{fieldsWithResolvers}}", fieldsWithResolvers)
        );

        context.AddSource($"{ns}{definition.TargetType}Controller.g.cs", builder.ToString());
    }


    private static string EmitFieldsWithResolvers(ObjectControllerDefinition definition)
    {
        var builder = new IndentedStringBuilder();
        for (var index = 0; index < definition.Properties.Count; index++)
        {
            ObjectPropertyDefinition property = definition.Properties[index];
            builder.Append(FieldWithResolverTemplate
                .Replace("{{fieldName}}", JsonNamingPolicy.CamelCase.ConvertName(property.Name))
                .Replace("{{fieldType}}", property.ClosestMatchingGraphQLTypeName)
                .Replace("{{resolverMethod}}", $"{definition.TargetType}Controller.{property.Name}")
            );

            if ((definition.Properties.Count > 1 && index < definition.Properties.Count - 1) ||
                definition.Methods.Count > 0)
                builder.AppendLine(",");
            else
                builder.AppendLine();


            if (index == 0)
            {
                builder.IncrementIndent();
                builder.IncrementIndent();
                builder.IncrementIndent();
                builder.IncrementIndent();
            }
        }

        for (var index = 0; index < definition.Methods.Count; index++)
        {
            ObjectMethodDefinition method = definition.Methods[index];

            string fieldName = JsonNamingPolicy.CamelCase.ConvertName(method.Name);
            string fieldType = method.ClosestMatchingGraphQLTypeName;
            var fieldArguments = method.Parameters
                .Where(p => p.FromArguments == true || p.IsPrimitive)
                .Select(a => $"{a.Name}: {a.ClosestMatchingGraphQLTypeName}")
                .ToList();

            string fieldDefinition =
                fieldArguments.Any() ? $"{fieldName}({string.Join(", ", fieldArguments)})" : fieldName;

            builder.Append(FieldWithResolverTemplate
                .Replace("{{fieldName}}", fieldDefinition)
                .Replace("{{fieldType}}", fieldType)
                .Replace("{{resolverMethod}}", $"{definition.TargetType}Controller.{method.Name}")
            );

            if (definition.Methods.Count > 1 && index < definition.Methods.Count - 1)
                builder.AppendLine(",");
            else
                builder.AppendLine();
        }

        return builder.ToString();
    }

    private static string EmitMethods(ObjectControllerDefinition definition)
    {
        if (!definition.Methods.Any())
            return string.Empty;

        var builder = new IndentedStringBuilder();
        builder.IncrementIndent();
        foreach (ObjectMethodDefinition method in definition.Methods)
        {
            bool isAsync = method.IsAsync;
            string asyncPrefix = isAsync ? "async " : string.Empty;
            builder.AppendLine($"public static {asyncPrefix}ValueTask {method.Name}(ResolverContext context)");
            builder.AppendLine("{");

            builder.IncrementIndent();

            if (!definition.IsStatic)
                builder.AppendLine($"var objectValue = ({definition.TargetType})context.ObjectValue;");

            string parameters = GetParameters(method);

            if (!definition.IsStatic)
                builder.AppendLine(isAsync
                    ? $"context.ResolvedValue = await objectValue.{method.Name}{parameters}"
                    : $"context.ResolvedValue = objectValue.{method.Name}{parameters}");
            else
                builder.AppendLine(isAsync
                    ? $"context.ResolvedValue = await {definition.TargetType}.{method.Name}{parameters}"
                    : $"context.ResolvedValue = {definition.TargetType}.{method.Name}{parameters}");

            if (!isAsync)
                builder.AppendLine("return default;");

            builder.DecrementIndent();
            builder.AppendLine("}");
            builder.AppendLine();
        }

        builder.DecrementIndent();
        return builder.ToString();
    }

    /// <summary>
    ///     public static ValueTask {{property}}(ResolverContext context)
    ///     {
    ///     var objectValue = ({{targetType}})context.ObjectValue;
    ///     context.ResolvedValue = objectValue.{{property}};
    ///     return default;
    ///     }
    /// </summary>
    private static string EmitProperties(ObjectControllerDefinition definition)
    {
        if (!definition.Properties.Any())
            return string.Empty;

        var builder = new IndentedStringBuilder();
        builder.IncrementIndent();
        foreach (ObjectPropertyDefinition property in definition.Properties)
        {
            builder.AppendLine($"public static ValueTask {property.Name}(ResolverContext context)");
            builder.AppendLine("{");
            builder.IncrementIndent();

            if (definition.IsStatic)
            {
                builder.AppendLine($"context.ResolvedValue = {definition.TargetType}.{property.Name};");
            }
            else
            {
                builder.AppendLine($"var objectValue = ({definition.TargetType})context.ObjectValue;");
                builder.AppendLine($"context.ResolvedValue = objectValue.{property.Name};");
            }

            builder.AppendLine("return default;");
            builder.DecrementIndent();
            builder.AppendLine("}");
            builder.AppendLine();
        }

        builder.DecrementIndent();
        return builder.ToString();
    }

    private static string GetParameters(ObjectMethodDefinition method)
    {
        if (!method.Parameters.Any()) return "();";

        var builder = new IndentedStringBuilder();
        builder.AppendLine("(");
        builder.IndentCount = 5;

        for (var index = 0; index < method.Parameters.Count; index++)
        {
            ParameterDefinition parameter = method.Parameters[index];

            if (parameter.Type.EndsWith("ResolverContext"))
            {
                builder.Append("context");
            }
            else if (parameter.Type.EndsWith("IServiceProvider"))
            {
                builder.Append("context.RequestServices");
            }
            else if (parameter.FromArguments == true)
            {
                builder.Append(parameter.IsPrimitive
                    ? $"context.GetArgument<{parameter.Type}>(\"{parameter.Name}\")"
                    : $"context.BindInputObject<{parameter.Type}>(\"{parameter.Name}\")");
            }
            else if (parameter.FromServices == true)
            {
                builder.Append(parameter.IsNullable
                    ? $"context.QueryContext.RequestServices.GetService<{parameter.Type}>()"
                    : $"context.GetRequiredService<{parameter.Type}>()");
            }
            else if (parameter.IsPrimitive)
            {
                builder.Append($"context.GetArgument<{parameter.Type}>(\"{parameter.Name}\")");
            }
            else
            {
                builder.Append($"context.HasArgument(\"{parameter.Name}\") ");
                builder.Append("? ");

                builder.Append(parameter.IsPrimitive
                    ? $"context.GetArgument<{parameter.Type}>(\"{parameter.Name}\")"
                    : $"context.BindInputObject<{parameter.Type}>(\"{parameter.Name}\")");

                builder.Append(": ");
                builder.Append($"context.GetRequiredService<{parameter.Type}>()");
            }

            if (method.Parameters.Count > 1 && index < method.Parameters.Count - 1)
                builder.AppendLine(",");
            else
                builder.AppendLine();
        }

        builder.AppendLine(");");
        return builder.ToString();
    }
}