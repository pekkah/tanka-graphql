using System;
using System.Collections.Generic;
using System.Linq;

using Tanka.GraphQL.Language.Nodes.TypeSystem;
using Tanka.GraphQL.ValueResolution;

namespace Tanka.GraphQL.TypeSystem;

/// <summary>
/// Implementation of schema build context
/// </summary>
public class SchemaBuildContext : ISchemaBuildContext
{
    public SchemaBuildContext(SchemaBuilder builder, SchemaBuildOptions options)
    {
        Builder = builder;
        Resolvers = new ResolversBuilder(options.Resolvers, options.Subscribers);
        Options = options;
        SchemaLoader = options.SchemaLoader;
        ServiceProvider = options.ServiceProvider ?? EmptyServiceProvider.Instance;
        Properties = new Dictionary<string, object>();
    }

    public SchemaBuilder Builder { get; }

    public ResolversBuilder Resolvers { get; }

    public SchemaBuildOptions Options { get; }

    public ISchemaLoader? SchemaLoader { get; }

    public IServiceProvider ServiceProvider { get; }

    public Dictionary<string, object> Properties { get; }

    public IEnumerable<TypeDefinition> QueryTypes(Func<TypeDefinition, bool>? predicate = null)
    {
        var types = Builder.GetTypeDefinitions();
        return predicate != null ? types.Where(predicate) : types;
    }

    public IEnumerable<DirectiveDefinition> QueryDirectives(Func<DirectiveDefinition, bool>? predicate = null)
    {
        var directives = Builder.GetDirectiveDefinitions();
        return predicate != null ? directives.Where(predicate) : directives;
    }

    public bool HasType(string name) => Builder.HasType(name);

    public TypeDefinition? GetType(string name) => Builder.GetTypeDefinition(name);
}

/// <summary>
/// Empty service provider for cases where no DI container is available
/// </summary>
internal class EmptyServiceProvider : IServiceProvider
{
    public static readonly EmptyServiceProvider Instance = new();

    public object? GetService(Type serviceType) => null;
}