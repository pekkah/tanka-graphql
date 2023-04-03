using Tanka.GraphQL.Language.Nodes.TypeSystem;
using Tanka.GraphQL.ValueResolution;

namespace Tanka.GraphQL.Executable;

public class ObjectDelegateResolversConfiguration : IExecutableSchemaConfiguration
{
    public ObjectDelegateResolversConfiguration(
        string type,
        IReadOnlyDictionary<FieldDefinition, Delegate> fields)
    {
        Type = type;
        Fields = fields;
    }

    public string Type { get; }

    public IReadOnlyDictionary<FieldDefinition, Delegate> Fields { get; }

    public Task Configure(SchemaBuilder schema, ResolversBuilder resolvers)
    {
        return Task.WhenAll(Configure(schema), Configure(resolvers));
    }

    private Task Configure(ResolversBuilder builder)
    {
        foreach (var (field, resolverDelegate) in Fields)
            builder.Resolver(Type, field.Name.Value).Run(resolverDelegate);

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