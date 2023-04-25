using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Tanka.GraphQL.Server.SourceGenerators;


[Generator]
public class ObjectTypeGenerator : IIncrementalGenerator
{
    public static string ObjectTypeAttributeName = "ObjectTypeAttribute";
    public static string ObjectTypeFullyQualifiedAttributeName = $"Tanka.GraphQL.Server.{ObjectTypeAttributeName}";

    public static string ObjectTypeSources = $$"""
        using System;
        using System.Threading.Tasks;
        using Microsoft.Extensions.Options;
        using Tanka.GraphQL.Executable;

        namespace Tanka.GraphQL.Server;

        public static class SourceGeneratedExecutableSchemaExtensions
        {
            public static OptionsBuilder<SchemaOptions> AddGeneratedTypes(
                this OptionsBuilder<SchemaOptions> builder,
                Action<SourceGeneratedTypesBuilder> configureTypes)
            {
                var typesBuilder = new SourceGeneratedTypesBuilder(builder);
                configureTypes(typesBuilder);
                return builder;
            }
        }

        public class SourceGeneratedTypesBuilder
        {
            public OptionsBuilder<SchemaOptions> Builder { get; }

            public SourceGeneratedTypesBuilder(OptionsBuilder<SchemaOptions> builder)
            {
                Builder = builder;
            }
        }

        [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
        public class {{ObjectTypeAttributeName}}: Attribute
        {
        }

        [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
        public class FromArgumentsAttribute: Attribute
        {
        }
        """;

    public static string InputTypeAttributeName = "InputTypeAttribute";
    public static string InputTypeFullyQualifiedAttributeName = $"Tanka.GraphQL.Server.{InputTypeAttributeName}";

    public static string InputTypeSources = $$"""
        using System;
        using System.Threading.Tasks;
        using Microsoft.Extensions.Options;
        using Tanka.GraphQL.Executable;

        namespace Tanka.GraphQL.Server;

        [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
        public class {{InputTypeAttributeName}}: Attribute
        {
        }
        """;

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        if (Debugger.IsAttached) Debugger.Break();

        context.RegisterPostInitializationOutput(c =>
        {
            c.AddSource("ObjectType.g.cs", ObjectTypeSources);
            c.AddSource("InputType.g.cs", InputTypeSources);
        });

        IncrementalValuesProvider<ObjectControllerDefinition> objectDefinitions = context.SyntaxProvider
                .ForAttributeWithMetadataName(
                    ObjectTypeFullyQualifiedAttributeName,
                    (node, ct) => node is ClassDeclarationSyntax,
                    (syntaxContext, ct) => ObjectTypeParser.ParseObjectControllerDefinition(
                        syntaxContext
                        )
                )
                .WithComparer(EqualityComparer<ObjectControllerDefinition>.Default)
                .WithTrackingName("ObjectTypes");


        context.RegisterSourceOutput(objectDefinitions, ObjectTypeEmitter.Emit);
        context.RegisterSourceOutput(objectDefinitions.Collect(), ObjectTypeEmitter.EmitNamespaceAddMethod);

        IncrementalValuesProvider<InputTypeDefinition> inputDefinitions = context
            .SyntaxProvider
                .ForAttributeWithMetadataName(
                    InputTypeFullyQualifiedAttributeName,
                    (node, ct) => node is ClassDeclarationSyntax,
                    (syntaxContext, ct) => new InputTypeParser(syntaxContext).ParseInputTypeDefinition(
                        (ClassDeclarationSyntax)syntaxContext.TargetNode
                    )
                )
                .WithComparer(EqualityComparer<InputTypeDefinition>.Default)
                .WithTrackingName("InputTypes");

        context.RegisterSourceOutput(inputDefinitions, (spc, inputDefinition) => new InputTypeEmitter(spc).Emit(inputDefinition));

        var inputDefinitionsByNamespace = inputDefinitions
            .Collect()
            .SelectMany((ns, _) =>
            {
                var group = ns.GroupBy(n => n.Namespace);
                return group;
            });
    }
}