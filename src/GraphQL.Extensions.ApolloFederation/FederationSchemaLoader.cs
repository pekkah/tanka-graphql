using System.Threading;
using System.Threading.Tasks;

using Tanka.GraphQL.Language.Nodes.TypeSystem;
using Tanka.GraphQL.TypeSystem;

namespace Tanka.GraphQL.Extensions.ApolloFederation;

/// <summary>
/// Schema loader for Apollo Federation specifications
/// </summary>
public class FederationSchemaLoader : ISchemaLoader
{
    /// <inheritdoc />
    public Task<TypeSystemDocument?> LoadSchemaAsync(string url, CancellationToken cancellationToken = default)
    {
        if (!CanLoad(url))
            return Task.FromResult<TypeSystemDocument?>(null);

        // Return the Federation v2.3 schema for all supported versions
        var federationSchema = @"
# Apollo Federation v2.3 Schema
# https://specs.apollo.dev/federation/v2.3

scalar _Any
scalar FieldSet

type _Service {
  sdl: String!
}

# Note: _Entity union and Query extensions for _entities/_service 
# are added dynamically by FederationConfigurationMiddleware based on actual entities

# Core Federation v1 directives
directive @external on FIELD_DEFINITION | OBJECT
directive @requires(fields: FieldSet!) on FIELD_DEFINITION
directive @provides(fields: FieldSet!) on FIELD_DEFINITION
directive @key(fields: FieldSet!, resolvable: Boolean = true) repeatable on OBJECT | INTERFACE

# Federation v2 directives
directive @shareable repeatable on OBJECT | FIELD_DEFINITION
directive @inaccessible on 
  | FIELD_DEFINITION 
  | OBJECT 
  | INTERFACE 
  | UNION 
  | ARGUMENT_DEFINITION 
  | SCALAR 
  | ENUM 
  | ENUM_VALUE 
  | INPUT_OBJECT 
  | INPUT_FIELD_DEFINITION

directive @tag(name: String!) repeatable on 
  | FIELD_DEFINITION 
  | INTERFACE 
  | OBJECT 
  | UNION 
  | ARGUMENT_DEFINITION 
  | SCALAR 
  | ENUM 
  | ENUM_VALUE 
  | INPUT_OBJECT 
  | INPUT_FIELD_DEFINITION

directive @override(from: String!) on FIELD_DEFINITION
directive @composeDirective(name: String!) repeatable on SCHEMA
directive @interfaceObject on OBJECT

# Security/Policy directives (Federation v2.1+)
directive @authenticated on 
  | FIELD_DEFINITION 
  | OBJECT 
  | INTERFACE 
  | SCALAR 
  | ENUM

directive @requiresScopes(scopes: [[federation__Scope!]!]!) on 
  | FIELD_DEFINITION 
  | OBJECT 
  | INTERFACE 
  | SCALAR 
  | ENUM

directive @policy(policies: [[federation__Policy!]!]!) on 
  | FIELD_DEFINITION 
  | OBJECT 
  | INTERFACE 
  | SCALAR 
  | ENUM

# Federation context directive (experimental)
directive @context(name: String!) repeatable on INTERFACE | OBJECT | UNION

# Federation scalars for security/policy
scalar federation__Scope
scalar federation__Policy
scalar federation__ContextFieldValue
";

        return url switch
        {
            "https://specs.apollo.dev/federation/v2.3" => Task.FromResult<TypeSystemDocument?>(federationSchema),
            "https://specs.apollo.dev/federation/v2.0" => Task.FromResult<TypeSystemDocument?>(federationSchema), // v2.0 compatibility
            "https://specs.apollo.dev/federation/v2.1" => Task.FromResult<TypeSystemDocument?>(federationSchema), // v2.1 compatibility  
            "https://specs.apollo.dev/federation/v2.2" => Task.FromResult<TypeSystemDocument?>(federationSchema), // v2.2 compatibility
            _ => Task.FromResult<TypeSystemDocument?>(null)
        };
    }

    /// <inheritdoc />
    public bool CanLoad(string url)
    {
        if (string.IsNullOrEmpty(url))
            return false;

        return url.StartsWith("https://specs.apollo.dev/federation/v2");
    }
}