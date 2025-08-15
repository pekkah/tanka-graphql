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

```csharp
// 1. Define schema with @link directive for Federation v2.3
var schema = @"
extend schema @link(url: ""https://specs.apollo.dev/federation/v2.3"", 
                   import: [""@key"", ""@external"", ""@requires"", ""@provides""])

type Product @key(fields: ""id"") {
    id: ID!
    name: String
    price: Float
}

type Query {
    product(id: ID!): Product
}";

// 2. Configure reference resolvers for entity resolution
var referenceResolvers = new DictionaryReferenceResolversMap
{
    [""Product""] = (context, type, representation) =>
    {
        var id = representation.GetValueOrDefault(""id"")?.ToString();
        var product = GetProductById(id); // Your data access logic
        return ValueTask.FromResult(new ResolveReferenceResult(type, product));
    }
};

// 3. Build executable schema with federation support
var executableSchema = new ExecutableSchemaBuilder()
    .Add(schema)
    .AddApolloFederation(new SubgraphOptions(referenceResolvers))
    .Build();
```

### Advanced Features

#### Schema Composition with @link

Apollo Federation v2.3 uses the `@link` directive for schema composition. Tanka GraphQL automatically processes these directives and imports the required types and directives:

```graphql
extend schema @link(url: "https://specs.apollo.dev/federation/v2.3", 
                   import: ["@key", "@external", "@requires", "@provides"])
```

#### Type Aliasing

You can use aliases to avoid naming conflicts when importing federation types:

```graphql
extend schema @link(url: "https://specs.apollo.dev/federation/v2.3", 
                   import: [{name: "@key", as: "@primaryKey"}, "@external"])

type Product @primaryKey(fields: "id") {
    id: ID!
}
```

#### Middleware Pipeline Integration

Federation support is seamlessly integrated into the schema builder's middleware pipeline:

```csharp
var schema = new SchemaBuilder()
    .Add(schemaSDL)
    .Build(options =>
    {
        options.Resolvers = resolvers;
        options.AddApolloFederation(subgraphOptions);
    });
```

The federation middleware automatically:
- Processes `@link` directives and imports required types
- Adds `_service` and `_entities` fields to the Query type
- Configures entity resolution based on your reference resolvers
- Generates proper subgraph SDL for federation

### Reference Resolvers

Reference resolvers handle entity resolution when the gateway requests entities by their key fields:

```csharp
var referenceResolvers = new DictionaryReferenceResolversMap
{
    ["Product"] = async (context, type, representation) =>
    {
        // Extract key fields from representation
        var id = representation.GetValueOrDefault("id")?.ToString();
        var sku = representation.GetValueOrDefault("sku")?.ToString();
        
        // Resolve entity based on key fields
        Product? product = null;
        if (id != null)
        {
            product = await productService.GetByIdAsync(id);
        }
        else if (sku != null)
        {
            product = await productService.GetBySkuAsync(sku);
        }
        
        return new ResolveReferenceResult(type, product);
    }
};
```

### Federation Directives

Tanka GraphQL supports all Apollo Federation v2.3 directives:

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

```diff
- # Federation v1 (deprecated)
- extend type Query {
-   _entities(representations: [_Any!]!): [_Entity]!
-   _service: _Service!
- }

+ # Federation v2.3
+ extend schema @link(url: "https://specs.apollo.dev/federation/v2.3", 
+                    import: ["@key", "@external"])
```

The middleware will automatically handle the migration and add the required fields.

### Best Practices

1. **Use specific imports** - Only import the federation directives you actually use
2. **Handle null entities** - Reference resolvers should handle cases where entities don't exist
3. **Optimize key selection** - Choose efficient key fields for entity resolution
4. **Test compatibility** - Use the Apollo Federation compatibility suite to validate your subgraph
5. **Monitor performance** - Entity resolution can impact query performance in large federations

### Troubleshooting

#### Common Issues

- **Missing @link directive**: Ensure your schema includes the @link directive with the correct Federation v2.3 URL
- **Entity resolution errors**: Check that your reference resolvers handle all key combinations
- **Type conflicts**: Use aliasing in @link imports to resolve naming conflicts
- **Schema validation errors**: Verify that all imported directives are properly used in your schema

#### Debug Mode

Enable detailed logging to troubleshoot federation issues:

```csharp
builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.SetMinimumLevel(LogLevel.Debug);
});
```
