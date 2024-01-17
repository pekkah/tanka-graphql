﻿using System.Runtime.Serialization;

using Microsoft.CodeAnalysis;
using System.Text;
using System.Text.Json;

using Microsoft.CodeAnalysis.CSharp;

namespace Tanka.GraphQL.Server.SourceGenerators;

public class InputTypeEmitter(SourceProductionContext context)
{
    public const string InputObjectTypeTemplate = 
        """
        /// <auto-generated/>
        #nullable enable
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
        
        {{parseableImplementation}}
        #nullable restore
        """;

    public static string ParseMethodTemplate(string name, string parseMethod) =>
        $$"""
        public partial class {{name}}: IParseableInputObject
        {
            public void Parse(IReadOnlyDictionary<string, object?> argumentValue)
            {
                {{parseMethod}}
            }
        }
        """;

    public static string TrySetProperty(string fieldName, string name, string type) =>
        $$"""
          // {{name}} is an scalar type
          if (argumentValue.TryGetValue("{{fieldName}}", out var {{fieldName}}Value))
          {
            if ({{fieldName}}Value is null)
            {
              {{name}} = default;
            }
            else 
            {
              {{name}} = ({{type}}){{fieldName}}Value;
            }
          }
          """;

    public static string TrySetPropertyObjectValue(string fieldName, string name, string type) =>
        $$"""
          // {{name}} is an input object type
          if (argumentValue.TryGetValue("{{fieldName}}", out var {{fieldName}}Value))
          {
              if ({{fieldName}}Value is null)
              {
                {{name}} = default;
              }
              else 
              {
                if ({{fieldName}}Value is not IReadOnlyDictionary<string, object?> dictionaryValue)
                    throw new InvalidOperationException($"{{fieldName}} is not IReadOnlyDictionary<string, object?>");
              
                {{name}} = new {{type}}();
                  
                if ({{name}} is not IParseableInputObject parseable)
                    throw new InvalidOperationException($"{{name}} is not IParseableInputObject");
                  
                parseable.Parse(dictionaryValue);
              }
          }
          """;

    public SourceProductionContext Context { get; } = context;

    public void Emit(InputTypeDefinition definition)
    {
        var typeSdl = BuildTypeSdl(definition);
        var builder = new StringBuilder();
        string ns = string.IsNullOrEmpty(definition.Namespace) ? "" : $"{definition.Namespace}";
        builder.AppendLine(InputObjectTypeTemplate
            .Replace("{{namespace}}", string.IsNullOrEmpty(ns) ? "" : $"namespace {ns};")
            .Replace("{{name}}", definition.TargetType)
            .Replace("{{typeSDL}}", typeSdl)
            .Replace("{{parseableImplementation}}", BuildParseMethod(definition))
        );

        var sourceText = CSharpSyntaxTree.ParseText(builder.ToString())
            .GetRoot()
            .NormalizeWhitespace()
            .ToFullString();
        
        Context.AddSource($"{ns}{definition.TargetType}InputType.g.cs", sourceText);
    }

    private string BuildParseMethod(InputTypeDefinition definition)
    {
        var builder = new IndentedStringBuilder();
        foreach (ObjectPropertyDefinition property in definition.Properties)
        {
            var typeName = property.ReturnType.Replace("?", "");
            var fieldName = JsonNamingPolicy.CamelCase.ConvertName(property.Name);
            if (property.ReturnTypeObject is not null)
            {
                builder.AppendLine(TrySetPropertyObjectValue(fieldName, property.Name, typeName));
            }
            else
            {
                builder.AppendLine(TrySetProperty(fieldName, property.Name, typeName));
            }
        }

        return ParseMethodTemplate(definition.TargetType, builder.ToString());
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