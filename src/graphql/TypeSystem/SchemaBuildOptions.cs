using Tanka.GraphQL.Directives;
using Tanka.GraphQL.TypeSystem.ValueSerialization;

namespace Tanka.GraphQL.TypeSystem;

public record SchemaBuildOptions
{
    public SchemaBuildOptions()
    {
    }

    public SchemaBuildOptions(IResolverMap resolvers, ISubscriberMap? subscribers = null)
    {
        Resolvers = resolvers;
        Subscribers = subscribers;
    }

    public bool BuildTypesFromOrphanedExtensions { get; set; } = false;

    public IReadOnlyDictionary<string, CreateDirectiveVisitor>? DirectiveVisitorFactories { get; set; }

    public bool IncludeBuiltInTypes { get; set; } = true;

    public bool IncludeIntrospection { get; set; } = true;

    public string? OverrideMutationRootName { get; set; }

    public string? OverrideQueryRootName { get; set; }

    public string? OverrideSubscriptionRootName { get; set; }

    public IResolverMap? Resolvers { get; set; }

    public ISubscriberMap? Subscribers { get; set; }

    public IReadOnlyDictionary<string, IValueConverter>? ValueConverters { get; set; } =
        new Dictionary<string, IValueConverter>
        {
            [Scalars.String.Name] = new StringConverter(),
            [Scalars.Int.Name] = new IntConverter(),
            [Scalars.Float.Name] = new DoubleConverter(),
            [Scalars.Boolean.Name] = new BooleanConverter(),
            [Scalars.ID.Name] = new IdConverter()
        };
}