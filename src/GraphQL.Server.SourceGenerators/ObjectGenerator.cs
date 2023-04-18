using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.CodeAnalysis;
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
        """;

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        if (Debugger.IsAttached) Debugger.Break();

        context.RegisterPostInitializationOutput(c =>
        {
            c.AddSource("ObjectType.g.cs", ObjectTypeSources);
        });

        IncrementalValuesProvider<ObjectControllerDefinition>
            schemaNodes = context.SyntaxProvider
                .ForAttributeWithMetadataName(
                    ObjectTypeFullyQualifiedAttributeName,
                    (node, ct) => node is ClassDeclarationSyntax,
                    (syntaxContext, ct) => ObjectTypeParser.ParseObjectControllerDefinition(
                        syntaxContext
                        )
                )
                .WithComparer(EqualityComparer<ObjectControllerDefinition>.Default)
                .WithTrackingName("ObjectType");


        context.RegisterSourceOutput(schemaNodes, ObjectTypeEmitter.Emit);
    }
}