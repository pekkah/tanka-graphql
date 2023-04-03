using Tanka.GraphQL.Directives;
using Tanka.GraphQL.Language.Nodes.TypeSystem;
using Tanka.GraphQL.ValueResolution;
using Tanka.GraphQL.ValueSerialization;

namespace Tanka.GraphQL.Executable;

public class ExecutableSchemaBuilder
{
    public SchemaBuilder Schema { get; }

    public ResolversBuilder Resolvers { get; }

    public ValueConvertersBuilder ValueConverters { get; }

    public Dictionary<string, CreateDirectiveVisitor> DirectiveVisitorFactories { get; }

    public ExecutableSchemaBuilder()
    {
        Schema = new SchemaBuilder();
        Resolvers = new ResolversBuilder();
        ValueConverters = new ValueConvertersBuilder()
            .AddDefaults();

        DirectiveVisitorFactories = new Dictionary<string, CreateDirectiveVisitor>();
    }

    public ExecutableSchemaBuilder Add(TypeSystemDocument document)
    {
        Schema.Add(document);
        return this;
    }

    public ExecutableSchemaBuilder Add(IExecutableSchemaConfiguration configuration)
    {
        configuration.Configure(Schema, Resolvers);
        return this;
    }

    public ExecutableSchemaBuilder Add(IResolverMap resolverMap)
    {
        Add(new ResolversConfiguration(resolverMap));
        return this;
    }

    public ExecutableSchemaBuilder Add(TypeDefinition[] types)
    {
        Schema.Add(types);
        return this;
    }

    public ExecutableSchemaBuilder Add(
        string typeName, 
        FieldsWithResolvers fields,
        FieldsWithSubscribers? subscribers = null)
    {
        Add(new ObjectResolversConfiguration(typeName, fields));

        if (subscribers != null)
            Add(new ObjectSubscribersConfiguration(typeName, subscribers));

        return this;
    }

    public ExecutableSchemaBuilder AddConverter(
        string typeName,
        IValueConverter valueConverter)
    {
        ValueConverters.Add(typeName, valueConverter);
        return this;
    }
    
    public async Task<ISchema> Build(Action<SchemaBuildOptions>? configureBuildOptions = null)
    {
        var buildOptions = new SchemaBuildOptions
        {
            Resolvers = Resolvers.BuildResolvers(),
            Subscribers = Resolvers.BuildSubscribers(),
            ValueConverters = ValueConverters.Build(),
            DirectiveVisitorFactories = DirectiveVisitorFactories.ToDictionary(kv => kv.Key, kv => kv.Value),
            BuildTypesFromOrphanedExtensions = true
        };

        configureBuildOptions?.Invoke(buildOptions);

        ISchema schema = await Schema.Build(buildOptions);

        return schema;
    }
}