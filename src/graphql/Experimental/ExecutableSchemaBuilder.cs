﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tanka.GraphQL.Experimental.Directives;
using Tanka.GraphQL.Experimental.TypeSystem;
using Tanka.GraphQL.Language.Nodes.TypeSystem;
using Tanka.GraphQL.TypeSystem.ValueSerialization;

namespace Tanka.GraphQL.Experimental;

public class ExecutableSchemaBuilder
{
    public List<ITypeSystemConfiguration> Configurations { get; } = new();

    public List<TypeSystemDocument> Documents { get; } = new();

    public Dictionary<string, IValueConverter> ValueConverters { get; } = new()
    {
        [Scalars.String.Name] = new StringConverter(),
        [Scalars.Int.Name] = new IntConverter(),
        [Scalars.Float.Name] = new DoubleConverter(),
        [Scalars.Boolean.Name] = new BooleanConverter(),
        [Scalars.ID.Name] = new IdConverter()
    };

    public Dictionary<string, CreateDirectiveVisitor> DirectiveVisitorFactories { get; } = new();

    public ExecutableSchemaBuilder AddTypeSystem(
        ITypeSystemConfiguration configuration)
    {
        Configurations.Add(configuration);

        return this;
    }

    public ExecutableSchemaBuilder AddTypeSystem(
        TypeSystemDocument document)
    {
        Documents.Add(document);

        return this;
    }

    public ExecutableSchemaBuilder ConfigureObject(
        string type,
        Dictionary<FieldDefinition, Action<ResolverBuilder>> fields)
    {
        Configurations.Add(new ObjectConfiguration(type, fields));
        return this;
    }

    public ExecutableSchemaBuilder AddValueConverter(string type, IValueConverter converter)
    {
        ValueConverters[type] = converter;
        return this;
    }

    public ExecutableSchemaBuilder AddDirectiveVisitor(string type, CreateDirectiveVisitor visitor)
    {
        DirectiveVisitorFactories[type] = visitor;
        return this;
    }

    public async Task<ISchema> Build(Action<SchemaBuildOptions>? configureBuildOptions = null)
    {
        var schemaBuilder = new TypeSystem.SchemaBuilder();
        var resolversBuilder = new ResolversBuilder();

        foreach (var typeSystemDocument in Documents)
            schemaBuilder.Add(typeSystemDocument);

        foreach (var configuration in Configurations)
        {
            await configuration.Configure(schemaBuilder, resolversBuilder);
        }

        var buildOptions = new SchemaBuildOptions
        {
            Resolvers = resolversBuilder.BuildResolvers(),
            Subscribers = resolversBuilder.BuildSubscribers(),
            ValueConverters = ValueConverters,
            DirectiveVisitorFactories = DirectiveVisitorFactories.ToDictionary(kv => kv.Key, kv => kv.Value),
            BuildTypesFromOrphanedExtensions = true
        };


        configureBuildOptions?.Invoke(buildOptions);

        var schema = await schemaBuilder.Build(buildOptions);

        return schema;
    }

    public ExecutableSchemaBuilder AddResolvers(IResolverMap resolversMap)
    {
        AddTypeSystem(new ResolversConfiguration(resolversMap));
        return this;
    }
}