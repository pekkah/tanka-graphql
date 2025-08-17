## Apollo Federation

"Implement a single data graph across multiple services"

> [Apollo Federation v2.3 Specification](https://www.apollographql.com/docs/federation/)

### Overview

Tanka GraphQL provides comprehensive support for Apollo Federation v2.3, implementing the subgraph specification that allows GraphQL services to be composed into a unified supergraph managed by an Apollo Gateway or Router.

Key features include:

- **Full Apollo Federation v2.3 compliance** with @link directive support
- **Middleware pipeline architecture** for seamless integration
- **Automatic schema generation** with proper SDL filtering
- **Type aliasing support** for @link imports to avoid naming conflicts
- **Reference resolvers** for entity federation
- **Built-in compatibility testing** with Apollo Federation subgraph compatibility suite

### Installation

Support is provided as a NuGet package:

```ps
dotnet add package Tanka.GraphQL.Extensions.ApolloFederation
```

### Quick Start

Creating a federated subgraph involves three main steps:

1. **Define your schema** with federation directives using `@link`
2. **Configure reference resolvers** for entity resolution
3. **Add federation support** to your schema builder

#### Basic Example

#include::xref://tests:GraphQL.Extensions.ApolloFederation.Tests/FederationDocumentationCodeExamples.cs?s=Tanka.GraphQL.Extensions.ApolloFederation.Tests.FederationDocumentationCodeExamples.BasicFederationExample

### Advanced Features

#### Schema Composition with @link

Apollo Federation v2.3 uses the `@link` directive for schema composition. Tanka GraphQL automatically processes these directives and imports the required types and directives.

#### Type Aliasing

You can use aliases to avoid naming conflicts when importing federation types. Aliasing allows you to rename imported directives and types:

```graphql
extend schema @link(url: "https://specs.apollo.dev/federation/v2.3", 
                   import: [{name: "@key", as: "@primaryKey"}, "@external"])

type Product @primaryKey(fields: "id") {
    id: ID!
    name: String
}
```

The `@link` directive supports importing with aliases using the object syntax:
- `{name: "@key", as: "@primaryKey"}` imports the `@key` directive as `@primaryKey`
- Simple string imports like `"@external"` use the original name

See the xref:types:14-link-directive.md[@link directive documentation] for complete details on schema composition and aliasing.

#### Middleware Pipeline Integration

Federation support is seamlessly integrated into the schema builder's middleware pipeline:

#include::xref://tests:GraphQL.Extensions.ApolloFederation.Tests/FederationDocumentationCodeExamples.cs?s=Tanka.GraphQL.Extensions.ApolloFederation.Tests.FederationDocumentationCodeExamples.MiddlewarePipelineIntegration

### Reference Resolvers

Reference resolvers handle entity resolution when the gateway requests entities by their key fields:

#include::xref://tests:GraphQL.Extensions.ApolloFederation.Tests/FederationDocumentationCodeExamples.cs?s=Tanka.GraphQL.Extensions.ApolloFederation.Tests.FederationDocumentationCodeExamples.ReferenceResolverExample

### Federation Directives

Tanka GraphQL supports all Apollo Federation v2.3 directives:

#include::xref://tests:GraphQL.Extensions.ApolloFederation.Tests/FederationDocumentationCodeExamples.cs?s=Tanka.GraphQL.Extensions.ApolloFederation.Tests.FederationDocumentationCodeExamples.FederationDirectivesExample

The supported directives include:
- `@key` - Define entity key fields
- `@external` - Mark fields as owned by other subgraphs  
- `@requires` - Specify required fields for computed fields
- `@provides` - Indicate which fields a resolver provides
- `@shareable` - Allow multiple subgraphs to resolve the same field
- `@inaccessible` - Hide fields from the public schema
- `@override` - Take ownership of a field from another subgraph
- `@tag` - Add metadata tags for tooling
- `@extends` - Extend types defined in other subgraphs
- `@composeDirective` - Include custom directives in composition
- `@interfaceObject` - Transform interfaces into object types

### Compatibility Testing

Tanka GraphQL includes comprehensive compatibility testing with the official Apollo Federation subgraph compatibility suite. You can run the compatibility tests for your own subgraph:

```bash
# Run the Apollo Federation compatibility sample
cd samples/GraphQL.Samples.ApolloFederation.Compatibility
dotnet run
```

### Migration from v1

If you're migrating from Apollo Federation v1, update your schema to use the `@link` directive:

#include::xref://tests:GraphQL.Extensions.ApolloFederation.Tests/FederationDocumentationCodeExamples.cs?s=Tanka.GraphQL.Extensions.ApolloFederation.Tests.FederationDocumentationCodeExamples.MigrationFromV1Example

### Best Practices

1. **Use specific imports** - Only import the federation directives you actually use:

#include::xref://tests:GraphQL.Extensions.ApolloFederation.Tests/FederationDocumentationCodeExamples.cs?s=Tanka.GraphQL.Extensions.ApolloFederation.Tests.FederationDocumentationCodeExamples.SpecificImportsExample

2. **Handle null entities** - Reference resolvers should handle cases where entities don't exist:

#include::xref://tests:GraphQL.Extensions.ApolloFederation.Tests/FederationDocumentationCodeExamples.cs?s=Tanka.GraphQL.Extensions.ApolloFederation.Tests.FederationDocumentationCodeExamples.ErrorHandlingExample

3. **Optimize key selection** - Choose efficient key fields for entity resolution
4. **Test compatibility** - Use the Apollo Federation compatibility suite to validate your subgraph
5. **Monitor performance** - Entity resolution can impact query performance in large federations

### Troubleshooting

#### Common Issues

- **Missing @link directive**: Ensure your schema includes the @link directive with the correct Federation v2.3 URL
- **Entity resolution errors**: Check that your reference resolvers handle all key combinations
- **Type conflicts**: Use aliasing in @link imports to resolve naming conflicts
- **Schema validation errors**: Verify that all imported directives are properly used in your schema

#### Federation Schema Loader

Tanka GraphQL includes a `FederationSchemaLoader` that automatically loads Federation v2.3 types when processing `@link` directives pointing to Apollo Federation specifications. This loader is automatically configured when you use `UseFederation()`.

### API Reference

#### SubgraphOptions

Configure your subgraph with reference resolvers:

```csharp
var options = new SubgraphOptions(referenceResolvers)
{
    // Optionally specify a different Federation version
    FederationSpecUrl = "https://specs.apollo.dev/federation/v2.3",
    
    // Optionally specify which types to import (null imports all)
    ImportList = new[] { "@key", "@external", "@requires" }
};
```

#### UseFederation Extension

Add Federation support to your schema builder:

```csharp
options.UseFederation(subgraphOptions);
```

This extension method:
- Adds Federation value converters for `_Any` and `FieldSet` scalars
- Configures the Federation schema loader
- Adds initialization middleware to process `@link` directives
- Adds configuration middleware to set up entity resolution