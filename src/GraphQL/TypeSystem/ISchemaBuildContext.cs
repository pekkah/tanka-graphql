using System;
using System.Collections.Generic;
using System.Linq;

using Tanka.GraphQL.Language.Nodes.TypeSystem;
using Tanka.GraphQL.ValueResolution;

namespace Tanka.GraphQL.TypeSystem;

/// <summary>
/// Context for schema build pipeline operations
/// </summary>
public interface ISchemaBuildContext
{
    /// <summary>
    /// The schema builder being used
    /// </summary>
    SchemaBuilder Builder { get; }

    /// <summary>
    /// Builder for resolvers
    /// </summary>
    ResolversBuilder Resolvers { get; }

    /// <summary>
    /// Schema build options and configuration
    /// </summary>
    SchemaBuildOptions Options { get; }

    /// <summary>
    /// Schema loader for @link directive processing
    /// </summary>
    ISchemaLoader? SchemaLoader { get; }

    /// <summary>
    /// Service provider for dependency injection
    /// </summary>
    IServiceProvider ServiceProvider { get; }

    /// <summary>
    /// Properties bag for passing data between pipeline stages
    /// </summary>
    Dictionary<string, object> Properties { get; }

    /// <summary>
    /// Query types in the schema with optional predicate
    /// </summary>
    IEnumerable<TypeDefinition> QueryTypes(Func<TypeDefinition, bool>? predicate = null);

    /// <summary>
    /// Query directives in the schema with optional predicate
    /// </summary>
    IEnumerable<DirectiveDefinition> QueryDirectives(Func<DirectiveDefinition, bool>? predicate = null);

    /// <summary>
    /// Check if a type exists in the schema
    /// </summary>
    bool HasType(string name);

    /// <summary>
    /// Get a specific type by name
    /// </summary>
    TypeDefinition? GetType(string name);
}