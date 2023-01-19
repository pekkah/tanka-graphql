﻿using Tanka.GraphQL.Language.Nodes.TypeSystem;

namespace Tanka.GraphQL;

public class ObjectSubscribersConfiguration : IExecutableSchemaConfiguration
{
    public ObjectSubscribersConfiguration(
        string type,
        IReadOnlyDictionary<FieldDefinition, Action<SubscriberBuilder>> fields)
    {
        Type = type;
        Fields = fields;
    }

    public string Type { get; }

    public IReadOnlyDictionary<FieldDefinition, Action<SubscriberBuilder>> Fields { get; }

    public Task Configure(SchemaBuilder schema, ResolversBuilder resolvers)
    {
        return Task.WhenAll(Configure(schema), Configure(resolvers));
    }

    private Task Configure(ResolversBuilder builder)
    {
        foreach (var (field, configureResolver) in Fields)
            configureResolver(builder.Subscriber(Type, field.Name));

        return Task.CompletedTask;
    }

    private Task Configure(SchemaBuilder builder)
    {
        var fields = Fields.Select(kv => kv.Key).ToList();

        // we add as type extension so we don't hit any conflicts with type names
        // WARNING: if schema is built with BuildTypesFromOrphanedExtensions set to false
        // the build will fail
        builder.Add(new TypeExtension(new ObjectDefinition(
            null,
            Type,
            fields: new(fields))));

        return Task.CompletedTask;
    }
}